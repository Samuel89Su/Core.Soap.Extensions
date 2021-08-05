using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SoapJsonConversionMiddleware
{
    public class SOAPMiddleware
    {
        private const string SOAP_HEADER_ACTION = "SOAPAction";
        private const string MEDIATYPE_JSON = "application/json; charset=utf-8";

        private const string SUFFIX_CONTROLLER = "controller";
        private const string SUFFIX_ACTION = "action";

        private readonly RequestDelegate _next;
        private readonly string _endpointPath;
        private readonly MessageEncoder _messageEncoder;
        private readonly ServiceDescription _service;

        private readonly string _routeTemplate;
        private ILogger _logger;

        public SOAPMiddleware(RequestDelegate next, Type serviceType, string path)
        {
            _next = next;
            _endpointPath = path;
            _messageEncoder = new BasicHttpBinding()
                .CreateBindingElements()
                .Find<MessageEncodingBindingElement>()?.CreateMessageEncoderFactory().Encoder;
            _service = new ServiceDescription(serviceType);

            var routeAttribute = serviceType.GetCustomAttribute<RouteAttribute>()
                ?? throw new ArgumentException($"Controller {serviceType.Name} must has RouteAttribute!");
            _routeTemplate = "/" + routeAttribute.Template
                .Replace($"[{SUFFIX_CONTROLLER}]", serviceType.Name.Replace(SUFFIX_CONTROLLER, string.Empty, StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase)
                .Replace($"[{SUFFIX_ACTION}]", "{0}", StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(_routeTemplate))
            {
                throw new ArgumentException($"Can not determine route template for Controller {serviceType.Name}!");
            }
        }

        public async Task Invoke(HttpContext httpContext)
        {
            _logger ??= (httpContext.RequestServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger<SOAPMiddleware>();

            var request = httpContext.Request;
            if (!request.Path.Equals(_endpointPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogDebug($"{request.Path.Value} do not match {_endpointPath}, go to next!");
                await _next(httpContext);
            }
            else
            {
                OperationDescription operationAction;
                object[] arguments = new object[0];
                var contentType = request.ContentType;
                var originResponseStream = httpContext.Response.Body;

                // read soap action from header
                var soapAction = request.Headers[SOAP_HEADER_ACTION].ToString().Trim('\"');

                using (var syncReadableReqBody = new MemoryStream())
                {
                    // copy to a sync readable stream.
                    await request.Body.CopyToAsync(syncReadableReqBody);
                    syncReadableReqBody.Position = 0;
                    syncReadableReqBody.Seek(0, SeekOrigin.Begin);

                    var requestMessage = _messageEncoder.ReadMessage(syncReadableReqBody, 0x10000, request.ContentType);

                    if (!string.IsNullOrEmpty(soapAction))
                    {
                        requestMessage.Headers.Action = soapAction;
                    }

                    // get operation action from registation
                    operationAction = _service.Operations.Where(o => o.SoapAction.Equals(requestMessage.Headers.Action, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (operationAction == null)
                    {
                        throw new InvalidOperationException($"No operation found for specified action: {requestMessage.Headers.Action}");
                    }
                    // deserialize operation action arguments from body
                    arguments = GetRequestArguments(requestMessage, operationAction);
                }

                // rewrite path
                var originPath = request.Path.Value;
                var newPath = string.Format(_routeTemplate, operationAction.Name);
                request.Path = newPath;
                request.ContentType = MEDIATYPE_JSON;

                // replace Request.Body with first argument
                var argument = JsonConvert.SerializeObject(arguments.FirstOrDefault());
                var jsonRequestBody = new MemoryStream(Encoding.UTF8.GetBytes(argument));
                jsonRequestBody.Seek(0, SeekOrigin.Begin);
                request.Body = jsonRequestBody;

                _logger?.LogDebug($"rewrite {originPath} to {newPath} with parameter {argument}!");

                var returntype = operationAction.DispatchMethod.ReturnType;
                if (returntype == typeof(Task))
                {
                    returntype = typeof(void);
                }
                else if (returntype.IsGenericType && typeof(Task).IsAssignableFrom(returntype))
                {
                    returntype = returntype.GenericTypeArguments.FirstOrDefault();
                }
                using (var readableResponseBody = new MemoryStream())
                {
                    // replace Response.Body with readable stream
                    httpContext.Response.Body = readableResponseBody;
                    await _next(httpContext);

                    if (httpContext.Response.StatusCode >= (int)HttpStatusCode.OK
                        && httpContext.Response.StatusCode < (int)HttpStatusCode.Ambiguous)
                    {
                        httpContext.Response.ContentType = contentType;
                        httpContext.Response.Headers[SOAP_HEADER_ACTION] = soapAction;
                        httpContext.Response.Body = originResponseStream;

                        readableResponseBody.Position = 0;
                        readableResponseBody.Seek(0, SeekOrigin.Begin);

                        object returnObject = null;
                        if (returntype != typeof(void))
                        {
                            using (var bodyReader = new StreamReader(readableResponseBody))
                            {
                                var res = await bodyReader.ReadToEndAsync();
                                // deserialize response
                                returnObject = JsonConvert.DeserializeObject(res, returntype);
                            }
                        }

                        var buffer = SoapXMLHandler.Serialize(returnObject, returntype, operationAction, _service.Contract.Namespace);
                        await httpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        private object[] GetRequestArguments(Message requestMessage, OperationDescription operation)
        {
            try
            {
                var parameters = operation.DispatchMethod.GetParameters();
                var arguments = new List<object>();

                // 反序列化请求包和对象
                using (var xmlReader = requestMessage.GetReaderAtBodyContents())
                {
                    // 查找的操作数据的元素
                    xmlReader.ReadStartElement(operation.Name, operation.Contract.Namespace);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        arguments.Add(SoapXMLHandler.Deserialize(xmlReader, parameters[i], operation.Contract.Namespace));
                    }
                }

                return arguments.ToArray();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public static class SOAPMiddlewareExtensions
    {
        public static IApplicationBuilder UseSOAPMiddleware<T>(this IApplicationBuilder builder, [NotNull] string path) where T : ControllerBase
        {
            return builder.UseMiddleware<SOAPMiddleware>(typeof(T), path);
        }
    }
}
