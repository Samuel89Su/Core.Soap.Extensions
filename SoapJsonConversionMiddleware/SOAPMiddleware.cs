using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace SoapJsonConversionMiddleware
{
    public class SOAPMiddleware
    {
        private const string SOAP_HEADER_ACTION = "SOAPAction";
        private const string MEDIATYPE_JSON = "application/json; charset=utf-8";
        private const string MEDIATYPE_FORM = "application/x-www-form-urlencoded";

        private const string TEMP_SUFFIX_CONTROLLER = "controller";
        private const string TEMP_SUFFIX_ACTION = "[action]";

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
                .Replace($"[{TEMP_SUFFIX_CONTROLLER}]", serviceType.Name.Replace(TEMP_SUFFIX_CONTROLLER, string.Empty, StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase);
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
                    operationAction = _service.Operations.Where(o => o.FullSoapAction.Equals(requestMessage.Headers.Action, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (operationAction == null)
                    {
                        throw new InvalidOperationException($"No operation found for specified action: {requestMessage.Headers.Action}");
                    }
                    // deserialize operation action arguments from body
                    arguments = GetRequestArguments(requestMessage, operationAction);
                }

                // rewrite path
                var originPath = request.Path.Value;
                request.Path = BuildRoutePath(operationAction);
                request.ContentType = MEDIATYPE_FORM;

                // replace Request.Body with first argument
                var argument = FormatArguments(arguments, operationAction.DispatchMethod.GetParameters());
                if (operationAction.DispatchMethod.GetParameters().Count() > 1)
                {
                    request.ContentType = MEDIATYPE_FORM;
                    request.QueryString += new QueryString("?" + argument);
                }
                else
                {
                    var jsonRequestBody = new MemoryStream(Encoding.UTF8.GetBytes(argument));
                    jsonRequestBody.Seek(0, SeekOrigin.Begin);
                    request.Body = jsonRequestBody;
                }

                _logger?.LogDebug($"rewrite {originPath} to {request.Path} with parameter {argument}!");

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

        private static readonly Regex regEx = new Regex("Async$");
        private string BuildRoutePath(OperationDescription operation)
        {
            var httpVerbAttribute = operation.DispatchMethod.GetCustomAttributes()
                .FirstOrDefault(a => typeof(HttpPostAttribute).IsAssignableFrom(a.GetType()))
                 as HttpPostAttribute;
            var actionName = regEx.Replace(operation.DispatchMethod.Name, string.Empty);

            var path = _routeTemplate.Replace(TEMP_SUFFIX_ACTION, actionName, StringComparison.OrdinalIgnoreCase);
            if (httpVerbAttribute != null && !string.IsNullOrWhiteSpace(httpVerbAttribute.Template))
            {
                var subPath = httpVerbAttribute.Template.Replace(TEMP_SUFFIX_ACTION, actionName, StringComparison.OrdinalIgnoreCase);
                return path + "/" + subPath;
            }

            return path;
        }

        private string FormatArguments(object[] arguments, ParameterInfo[] parameters)
        {
            if (arguments == null || arguments.Length == 0)
            {
                return string.Empty;
            }
            if (arguments.Length == 1)
            {
                return JsonConvert.SerializeObject(arguments.FirstOrDefault());
            }
            else
            {
                var values = new Dictionary<string, string>();
                for (var i = 0; i < parameters.Count(); i++)
                {
                    var parameter = parameters[i];
                    var argument = arguments.Skip(i).FirstOrDefault();
                    if (argument != null)
                    {
                        if (parameter.ParameterType.IsPrimitive || parameter.ParameterType == typeof(Guid) || parameter.ParameterType == typeof(string))
                        {
                            values[parameter.Name] = HttpUtility.UrlEncode(argument.ToString());
                        }
                        else if (parameter.ParameterType.IsClass && !typeof(IEnumerable).IsAssignableFrom(parameter.ParameterType))
                        {
                            foreach (var token in JToken.FromObject(argument).Children().Cast<JProperty>())
                            {
                                values[string.Concat(parameter.Name, ".", token.Name)] = HttpUtility.UrlEncode(token.ToString());
                            }
                        }
                    }
                }
                return string.Join("&", values.Select(v => string.Concat(v.Key, "=", v.Value)));
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
                    xmlReader.ReadStartElement(operation.SoapAction, operation.Contract.Namespace);
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
