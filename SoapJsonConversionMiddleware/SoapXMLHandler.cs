using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SoapJsonConversionMiddleware
{
    public class SoapXMLHandler
    {
        public static object Deserialize(XmlDictionaryReader xmlReader, ParameterInfo parameterInfo, string @namespace)
        {
            var parameterName = parameterInfo.GetCustomAttribute<MessageParameterAttribute>()?.Name ?? parameterInfo.Name;
            var parameterType = parameterInfo.ParameterType;

            var outerXml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);

            var xmlSerializer = new XmlSerializer(parameterType);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(outerXml)))
            {
                return xmlSerializer.Deserialize(ms);
            }
        }

        public static string OverwriteSoapXml(Type parameterType, string parameterName, string @namespace, XmlDictionaryReader xmlReader)
        {
            xmlReader.MoveToStartElement(parameterName, @namespace);
            if (xmlReader.IsStartElement(parameterName, @namespace))
            {
                var outerXml = xmlReader.ReadOuterXml();
                outerXml = outerXml.Substring(outerXml.IndexOf(">") + 1);
                outerXml = outerXml.Substring(0, outerXml.LastIndexOf("<"));
                var typeNodeName = GetTypeNodeName(parameterType);
                outerXml = $"<{typeNodeName}>" + outerXml + $"</{typeNodeName}>";

                return outerXml;
            }
            else
                return string.Empty;
        }

        private static string GetTypeNodeName(Type type)
        {
            var typeName = type.Name;
            if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                var firstGenericTypeArgument = type.GenericTypeArguments.FirstOrDefault();
                return "ArrayOf" + firstGenericTypeArgument.Name;
            }
            else if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
            {
                return ToLowerCamelCase(typeName);
            }

            return typeName;
        }

        private static string ToLowerCamelCase(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }
            return char.ToLower(source[0]) + source.Substring(1);
        }

        private const string XML_Envelope = "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><GetAccountResponse xmlns=\"{1}\">{0}</GetAccountResponse></soap:Body></soap:Envelope>";
        public static byte[] Serialize(object data, Type returnType, OperationDescription operationAction, string @namespace)
        {
            var resultName = operationAction.DispatchMethod.ReturnParameter.GetCustomAttribute<MessageParameterAttribute>()?.Name ?? operationAction.Name + "Result";

            var response = string.Empty;
            if (returnType == typeof(void) || data == null)
            {
                response = string.Format(XML_Envelope, string.Empty, @namespace);
            }
            else
            {
                var xml = string.Empty;
                using (var ms = new MemoryStream())
                {
                    var xmlSerializer = new XmlSerializer(returnType);
                    xmlSerializer.Serialize(ms, data);
                    ms.Position = 0;
                    using (var reader = new StreamReader(ms))
                    {
                        xml = reader.ReadToEnd();
                    }
                }
                xml = xml.Substring(xml.IndexOf('>') + 1);
                xml = xml.Substring(xml.IndexOf('>') + 1);
                xml = xml.Substring(0, xml.LastIndexOf('<'));
                xml = $"<{resultName}>" + xml + $"</{resultName}>";
                response = string.Format(XML_Envelope, xml, @namespace);
            }
            return Encoding.UTF8.GetBytes(response);
        }
    }
}
