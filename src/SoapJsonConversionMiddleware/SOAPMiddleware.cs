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

namespace SoapJsonConversion.Middleware
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

                var requestBody = OverwriteRequest(request, arguments, operationAction, _routeTemplate);

                _logger?.LogDebug($"rewrite {originPath} to {request.Path} with parameter {requestBody}!");

                var returntype = GetReturnType(operationAction);
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
                                returnObject = ParseReturn(res, returntype);
                            }
                        }

                        var response = SoapXMLHandler.Envelope(SoapXMLHandler.Serialize(returnObject, operationAction.DispatchMethod.ReturnParameter, returntype, operationAction.SoapAction), operationAction.SoapAction, _service.Contract.Namespace);
                        await httpContext.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(response), 0, response.Length);
                    }
                }
            }
        }

        private static Type GetReturnType(OperationDescription operationAction)
        {
            var returntype = operationAction.DispatchMethod.ReturnType;
            if (returntype == typeof(Task))
            {
                returntype = typeof(void);
            }
            else if (returntype.IsGenericType && typeof(Task).IsAssignableFrom(returntype))
            {
                returntype = returntype.GenericTypeArguments.FirstOrDefault();
            }

            return returntype;
        }

        private static object ParseReturn(string result, Type type)
        {
            if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
            {
                return Convert.ChangeType(result, type);
            }
            else
            {
                return JToken.Parse(result).ToObject(type);
            }
        }

        private static string OverwriteRequest(HttpRequest httpRequest, object[] arguments, OperationDescription operationDescription, string routeTemplate)
        {
            httpRequest.Path = BuildRoutePath(operationDescription.DispatchMethod, routeTemplate);
            httpRequest.ContentType = MEDIATYPE_JSON;

            // replace Request.Body with first argument
            var requestBody = FormatArguments(arguments, operationDescription.DispatchMethod.GetParameters());

            var jsonRequestBody = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
            jsonRequestBody.Seek(0, SeekOrigin.Begin);
            httpRequest.Body = jsonRequestBody;

            return requestBody;
        }

        private static readonly Regex regEx = new Regex("Async$");
        private static string BuildRoutePath(MethodInfo method, string routeTemplate)
        {
            var httpVerbAttribute = method.GetCustomAttributes()
                .FirstOrDefault(a => typeof(HttpPostAttribute).IsAssignableFrom(a.GetType()))
                 as HttpPostAttribute;
            var actionName = regEx.Replace(method.Name, string.Empty);

            var path = routeTemplate.Replace(TEMP_SUFFIX_ACTION, actionName, StringComparison.OrdinalIgnoreCase);
            if (httpVerbAttribute != null && !string.IsNullOrWhiteSpace(httpVerbAttribute.Template))
            {
                var subPath = httpVerbAttribute.Template.Replace(TEMP_SUFFIX_ACTION, actionName, StringComparison.OrdinalIgnoreCase);
                return path + "/" + subPath;
            }

            return path;
        }

        private static string FormatArguments(object[] arguments, ParameterInfo[] parameters)
        {
            if (arguments == null || arguments.Length == 0 || arguments.All(a => a == null))
            {
                return string.Empty;
            }
            if (parameters.Count() > 1)
            {
                throw new InvalidOperationException($"Action should be only one parameter, but get {parameters.Count()}!");
            }
            var parameter = parameters.First();
            return JsonConvert.SerializeObject(arguments.FirstOrDefault());
        }

        private object[] GetRequestArguments(Message requestMessage, OperationDescription operation)
        {
            try
            {
                var arguments = new List<object>();
                var parameters = operation.DispatchMethod.GetParameters();
                if (parameters == null || parameters.Count() == 0)
                {
                    return arguments.ToArray();
                }
                if (parameters.Count() > 1)
                {
                    throw new InvalidOperationException($"{operation.DispatchMethod.DeclaringType.Name}.{operation.DispatchMethod.Name} must be a single parameter method, but with parameters {string.Join(",", parameters.Select(p => p.Name))}!");
                }

                var parameter = parameters.First();
                using (var xmlReader = requestMessage.GetReaderAtBodyContents())
                {
                    arguments.Add(SoapXMLHandler.Deserialize(xmlReader, parameter, operation.SoapAction, operation.Contract.Namespace));
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
