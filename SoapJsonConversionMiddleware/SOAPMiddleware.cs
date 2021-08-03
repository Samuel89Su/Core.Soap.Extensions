using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
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
            if (!httpContext.Request.Path.Equals(_endpointPath, StringComparison.OrdinalIgnoreCase))
            {
                await _next(httpContext);
            }
            else
            {
                OperationDescription operationAction;
                object[] arguments = new object[0];
                var contentType = httpContext.Request.ContentType;
                var originResponseStream = httpContext.Response.Body;

                // read soap action from header
                var soapAction = httpContext.Request.Headers[SOAP_HEADER_ACTION].ToString().Trim('\"');

                using (var syncReadableReqBody = new MemoryStream())
                {
                    // copy to a sync readable stream.
                    await httpContext.Request.Body.CopyToAsync(syncReadableReqBody);
                    syncReadableReqBody.Position = 0;
                    syncReadableReqBody.Seek(0, SeekOrigin.Begin);

                    var requestMessage = _messageEncoder.ReadMessage(syncReadableReqBody, 0x10000, httpContext.Request.ContentType);

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
                httpContext.Request.Path = string.Format(_routeTemplate, operationAction.Name);
                httpContext.Request.ContentType = MEDIATYPE_JSON;

                // replace Request.Body with first argument
                var jsonRequestBody = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(arguments.FirstOrDefault())));
                jsonRequestBody.Seek(0, SeekOrigin.Begin);
                httpContext.Request.Body = jsonRequestBody;

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
                        using (var bodyReader = new StreamReader(readableResponseBody))
                        {
                            var res = await bodyReader.ReadToEndAsync();
                            // deserialize response
                            var responseObject = JsonConvert.DeserializeObject(res, operationAction.DispatchMethod.ReturnType);

                            var buffer = EnvelopeMessage(responseObject, operationAction);
                            await httpContext.Response.Body.WriteAsync(buffer, 0, buffer.Length);
                        }
                    }
                }
            }
        }

        private object[] GetRequestArguments(Message requestMessage, OperationDescription operation)
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
                    var parameterName = parameters[i].GetCustomAttribute<MessageParameterAttribute>()?.Name ?? parameters[i].Name;
                    xmlReader.MoveToStartElement(parameterName, operation.Contract.Namespace);
                    if (xmlReader.IsStartElement(parameterName, operation.Contract.Namespace))
                    {
                        var serializer = new DataContractSerializer(parameters[i].ParameterType, parameterName, operation.Contract.Namespace);
                        arguments.Add(serializer.ReadObject(xmlReader, verifyObjectName: true));
                    }
                }
            }

            return arguments.ToArray();
        }

        private const string XML_Envelope = "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><GetAccountResponse xmlns=\"{1}\">{0}</GetAccountResponse></soap:Body></soap:Envelope>";
        private byte[] EnvelopeMessage(object responseObject, OperationDescription operationAction)
        {
            var resultName = operationAction.DispatchMethod.ReturnParameter.GetCustomAttribute<MessageParameterAttribute>()?.Name ?? operationAction.Name + "Result";

            var response = string.Empty;
            using (var ms = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(operationAction.DispatchMethod.ReturnType);
                xmlSerializer.Serialize(ms, responseObject);
                ms.Position = 0;
                using (var reader = new StreamReader(ms))
                {
                    var str = reader.ReadToEnd();
                    var bodyIdx = str.IndexOf('>') + 1;
                    str = str.Substring(bodyIdx);
                    bodyIdx = str.IndexOf('>') + 1;
                    str = str.Substring(bodyIdx);
                    var bodyEndIdx = str.LastIndexOf('<');
                    str = str.Substring(0, bodyEndIdx);
                    str = $"<{resultName}>" + str + $"</{resultName}>";
                    response = string.Format(XML_Envelope, str, _service.Contract.Namespace);
                }
            }
            return Encoding.UTF8.GetBytes(response);
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
