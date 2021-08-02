//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using SoapXMLParser;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace TestCoreApplication
//{
//    public class SoapMiddleware : IMiddleware
//    {
//        private const string CONTENT_TYPE_JSON = "application/json";

//        private readonly IDictionary<string, string> _serviceRewrite = new Dictionary<string, string>();
//        private readonly IDictionary<string, string> _actionRewrite = new Dictionary<string, string>();

//        //public IDictionary<string, string> ServiceRewrite { get => _serviceRewrite; set { _serviceRewrite = value; } }
//        //public IDictionary<string, string> ActionRewrite { get => _actionRewrite; set { _actionRewrite = value; } }

//        public SoapMiddleware(IDictionary<string, string> serviceRewrite, IDictionary<string, string> actionRewrite)
//        {
//            _serviceRewrite = new Dictionary<string, string>(serviceRewrite, StringComparer.OrdinalIgnoreCase);
//            _actionRewrite = new Dictionary<string, string>(actionRewrite, StringComparer.OrdinalIgnoreCase);
//        }

//        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
//        {
//            var request = context.Request;

//            var soapXMLDescriptor = await GetSoapXMLDescriptor(context);
//            if (soapXMLDescriptor == null)
//            {
//                await next(context);
//                return;
//            }

//            if (!_serviceRewrite.TryGetValue(request.Path.Value.Split('/').LastOrDefault(), out string path) || string.IsNullOrWhiteSpace(path))
//            {
//                await next(context);
//                return;
//            }
//            if (!_actionRewrite.TryGetValue(soapXMLDescriptor.Action, out string action) || string.IsNullOrWhiteSpace(action))
//            {
//                await next(context);
//                return;
//            }

//            // rewrite path
//            request.Path = new PathString(path) + action;

//            // rewrite body
//            var argumentType = GetActionArgumentType(path.Split('/').LastOrDefault(), action.Trim('/'), Assembly.GetEntryAssembly());
//            if (!soapXMLDescriptor.Payload.TryDeserialize(argumentType, out object value))
//            {
//                await next(context);
//                return;
//            }

//            var newReqBody = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
//            newReqBody.Seek(0, SeekOrigin.Begin);
//            context.Request.Body = newReqBody;
//            context.Request.ContentType = CONTENT_TYPE_JSON;
//        }

//        private static async Task<SoapXMLDescriptor> GetSoapXMLDescriptor(HttpContext context)
//        {
//            var request = context.Request;

//            var couldBeSoapRequest = true
//                && HttpMethods.IsPost(request.Method)
//                && (request.ContentType.StartsWith(SoapXMLDescriptor.CONTENT_TYPE_TEXT_XML, StringComparison.OrdinalIgnoreCase)
//                    || request.ContentType.StartsWith(SoapXMLDescriptor.CONTENT_TYPE_SOAP_XML, StringComparison.OrdinalIgnoreCase));
//            if (couldBeSoapRequest)
//            {
//                var xml = new XmlDocument();
//                using (var stream = new MemoryStream())
//                {
//                    await request.Body.CopyToAsync(stream);
//                    stream.Position = 0;
//                    xml.Load(stream);
//                }

//                couldBeSoapRequest &= xml.DocumentElement.TryDeenvelope(out SoapXMLDescriptor soapXMLDescriptor);

//                if (couldBeSoapRequest && soapXMLDescriptor != null)
//                {
//                    return soapXMLDescriptor;
//                }
//            }

//            return null;
//        }

//        //private const string HEADER_CONTENTTYPE_CHARSET = "charset";
//        //private static Encoding GetEncodingOrDefault(string contentType)
//        //{
//        //    var encoding = Encoding.UTF8;
//        //    if (string.IsNullOrWhiteSpace(contentType))
//        //    {
//        //        return encoding;
//        //    }

//        //    var charsetIndex = contentType.IndexOf(HEADER_CONTENTTYPE_CHARSET, StringComparison.OrdinalIgnoreCase);
//        //    if (charsetIndex < 0)
//        //    {
//        //        return encoding;
//        //    }

//        //    var charset = contentType.Substring(charsetIndex).Split('=').Skip(1).FirstOrDefault();
//        //    if (string.IsNullOrWhiteSpace(charset))
//        //    {
//        //        return encoding;
//        //    }

//        //    if (charset.StartsWith(Encoding.UTF7.WebName, StringComparison.OrdinalIgnoreCase))
//        //    {
//        //        encoding = Encoding.UTF7;
//        //    }
//        //    else if (charset.StartsWith(Encoding.UTF32.WebName, StringComparison.OrdinalIgnoreCase))
//        //    {
//        //        encoding = Encoding.UTF32;
//        //    }
//        //    else if (charset.StartsWith(Encoding.Unicode.WebName, StringComparison.OrdinalIgnoreCase))
//        //    {
//        //        encoding = Encoding.Unicode;
//        //    }
//        //    else if (charset.StartsWith(Encoding.ASCII.WebName, StringComparison.OrdinalIgnoreCase))
//        //    {
//        //        encoding = Encoding.ASCII;
//        //    }
//        //    else if (charset.StartsWith(Encoding.BigEndianUnicode.WebName, StringComparison.OrdinalIgnoreCase))
//        //    {
//        //        encoding = Encoding.BigEndianUnicode;
//        //    }

//        //    return encoding;
//        //}

//        private static Type GetActionArgumentType(string controllerName, string action, Assembly assembly = null)
//        {
//            assembly ??= typeof(SoapMiddleware).Assembly;

//            var controller = assembly.GetTypes().FirstOrDefault(t => typeof(ControllerBase).IsAssignableFrom(t) && t.Name.Equals(controllerName + "controller", StringComparison.OrdinalIgnoreCase));
//            if (controller == null)
//            {
//                return null;
//            }

//            var method = controller.GetMethod(action);
//            if (method == null)
//            {
//                return null;
//            }

//            return method.GetParameters().FirstOrDefault(p => p.ParameterType.IsClass)?.ParameterType;
//        }
//    }

//    public class SoapMiddlewareOption
//    {
//        private IDictionary<string, string> _serviceRewrite = new Dictionary<string, string>();
//        private IDictionary<string, string> _actionRewrite = new Dictionary<string, string>();

//        public IDictionary<string, string> ServiceRewrite { get => _serviceRewrite; set { _serviceRewrite = value; } }
//        public IDictionary<string, string> ActionRewrite { get => _actionRewrite; set { _actionRewrite = value; } }
//    }
//}
