using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SoapJsonConversion
{
    public class SoapXMLHandler
    {
        private const string XML_Envelope = "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><GetAccountResponse xmlns=\"{1}\">{0}</GetAccountResponse></soap:Body></soap:Envelope>";

        public static object[] ParseJsonToArguments(JToken jToken, ParameterInfo[] parameters)
        {
            var arguments = new List<object>();
            if (parameters == null || parameters.Length == 0)
            {
                return arguments.ToArray();
            }
            if (jToken.Type == JTokenType.Null)
            {
                return arguments.ToArray();
            }
            if (parameters.Length == 1)
            {
                var firstParameterType = parameters.First().ParameterType;
                return new object[] { jToken.ToObject(firstParameterType) };
            }
            else if (jToken.Type == JTokenType.Object)
            {
                foreach (var jProperty in jToken.Children().Cast<JProperty>())
                {
                    var parameter = parameters.FirstOrDefault(p => p.Name.Equals(jProperty.Name, StringComparison.OrdinalIgnoreCase));
                    if (parameter != null)
                    {
                        arguments.AddRange(ParseJsonToArguments(jProperty.Value, new ParameterInfo[] { parameter }));
                    }
                }
            }
            return arguments.ToArray();
        }

        public static object Deserialize(XmlDictionaryReader xmlReader, ParameterInfo parameterInfo, string @namespace)
        {
            var parameterName = parameterInfo.GetCustomAttribute<MessageParameterAttribute>()?.Name ?? parameterInfo.Name;
            var parameterType = parameterInfo.ParameterType;

            var xml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);

            var xmlSerializer = new XmlSerializer(parameterType);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return xmlSerializer.Deserialize(ms);
            }
        }

        public static string Serialize(object data, ParameterInfo parameterInfo, Type type, string soapAction, string @namespace)
        {
            var resultName = parameterInfo.GetCustomAttribute<MessageParameterAttribute>()?.Name ?? soapAction + "Result";

            var response = string.Empty;
            if (type == typeof(void) || data == null)
            {
                response = string.Format(XML_Envelope, string.Empty, @namespace);
            }
            else
            {
                var xml = string.Empty;
                using (var ms = new MemoryStream())
                {
                    var xmlSerializer = new XmlSerializer(type);
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
            return response;
        }

        public static string OverwriteSoapXml(Type parameterType, string parameterName, string @namespace, XmlDictionaryReader xmlReader)
        {
            if (!xmlReader.IsStartElement(parameterName, @namespace))
            {
                // try move to
                try
                {
                    xmlReader.MoveToStartElement(parameterName, @namespace);
                }
                catch (XmlException ex)
                {
                }
            }
            if (xmlReader.IsStartElement(parameterName, @namespace))
            {
                var outerXml = xmlReader.ReadOuterXml();
                outerXml = outerXml.Substring(outerXml.IndexOf(">") + 1);
                outerXml = outerXml.Substring(0, outerXml.LastIndexOf("<"));
                var typeNodeName = GetTypeNodeName(parameterType);
                outerXml = $"<{typeNodeName}>" + outerXml + $"</{typeNodeName}>";

                return outerXml;
            }
            else if (parameterType.IsClass && typeof(string) != parameterType)
            {
                var outerXml = string.Empty;
                foreach (var property in parameterType.GetProperties().Where(p => p.GetSetMethod().IsPublic))
                {
                    var xml = xmlReader.ReadOuterXml();
                    if (string.IsNullOrWhiteSpace(xml.Trim()))
                    {
                        break;
                    }
                    outerXml += xml.Replace($"xmlns=\"{@namespace}\"", string.Empty);
                    if (!xmlReader.IsStartElement())
                    {
                        break;
                    }
                }
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
    }
}
