using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Shouldly;
using SoapJsonConversion;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using SoapJsonConversion.Model;
using System.Reflection;
using System.ServiceModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Dynamic;

namespace SoapJsonConversion.Tests
{
    public class SoapXMLHandlerTests
    {
        [Fact]
        public void ParseJsonToArguments_NullOrEmpty_Test()
        {
            ParameterInfo[] parameters = null;
            var jtoken = JToken.Parse("{\"auth\":\"autho\",\"text\":\"tttrttr\"}");

            var arguments = SoapXMLHandler.ParseJsonToArguments(jtoken, parameters);

            arguments.ShouldNotBeNull();
            arguments.Length.ShouldBe(0);

            parameters = new ParameterInfo[] { };
            arguments = SoapXMLHandler.ParseJsonToArguments(jtoken, parameters);

            arguments.ShouldNotBeNull();
            arguments.Length.ShouldBe(0);

            parameters = typeof(AndroidStructNotification).GetMethod("Detect").GetParameters();
            jtoken = JToken.Parse("null");

            arguments = SoapXMLHandler.ParseJsonToArguments(jtoken, parameters);

            arguments.ShouldNotBeNull();
            arguments.Length.ShouldBe(0);
        }

        [Fact]
        public void ParseJsonToArguments_Detect_Test()
        {
            var parameters = typeof(AndroidStructNotification).GetMethod("Detect").GetParameters();

            var jtoken = JToken.Parse("{\"auth\":\"autho\",\"text\":\"tttrttr\"}");

            var arguments = SoapXMLHandler.ParseJsonToArguments(jtoken, parameters);

            arguments.ShouldNotBeNull();
            arguments.Length.ShouldBe(2);
            arguments[0].ShouldBe("autho");
            arguments[1].ShouldBe("tttrttr");
        }

        [Fact]
        public void ParseJsonToArguments_Translate_Test()
        {
            var parameters = typeof(AndroidStructNotification).GetMethod("Translate").GetParameters();

            var jtoken = JToken.Parse("{\"auth\":\"autho\",\"texts\":[\"tttrttr\",\"yterytryt\"],\"sourceLanguage\":\"en\",\"targetLanguage\":\"ch\",\"isHtml\":true}");

            var arguments = SoapXMLHandler.ParseJsonToArguments(jtoken, parameters);

            arguments.ShouldNotBeNull();
            arguments.Length.ShouldBe(5);
            arguments[0].ShouldBe("autho");
            arguments[1].ShouldBeOfType<string[]>();
            (arguments[1] as string[]).Length.ShouldBe(2);
            arguments[2].ShouldBe("en");
            arguments[3].ShouldBe("ch");
            arguments[4].ShouldBe(true);

            jtoken = JToken.Parse("{\"auth\":\"autho\",\"texts\":[\"tttrttr\",\"yterytryt\"],\"sourceLanguage\":\"en\",\"targetLanguage\":\"ch\"}");

            arguments = SoapXMLHandler.ParseJsonToArguments(jtoken, parameters);

            arguments.ShouldNotBeNull();
            arguments.Length.ShouldBe(4);
            arguments[0].ShouldBe("autho");
            arguments[1].ShouldBeOfType<string[]>();
            (arguments[1] as string[]).Length.ShouldBe(2);
            arguments[2].ShouldBe("en");
            arguments[3].ShouldBe("ch");
        }

        [Fact]
        public void ParseJsonToArguments_NewPushNotification_Test()
        {
            try
            {
                var parameters = typeof(AndroidStructNotification).GetMethod("NewPushNotification").GetParameters();
                var jtoken = JToken.Parse("[{\"_androidToken\":\"564f5456d\",\"_type\":1,\"_alertMessage\":\"ghdegredgre\",\"_visitorId\":543543},{\"_androidToken\":\"fcm:\",\"_type\":1,\"_alertMessage\":\"ghdegredgre\",\"_visitorId\":543543,\"_visitorGuid\":\"29dad7b2-7d9c-46aa-b8ed-19386514175e\",\"_partnerId\":\"fsafdsgfdsghrfd\"}]");

                var arguments = SoapXMLHandler.ParseJsonToArguments(jtoken, parameters);

                arguments.ShouldNotBeNull();
                arguments.Length.ShouldBe(1);
                var notifications = arguments.First() as AndroidStructNotification[];
                notifications.Count().ShouldBe(2);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        const string @namespace = "http://webservice.com";

        [Fact(DisplayName = "Deserialize Guid")]
        public void OverwriteSoapXml_Guid_Test()
        {
            try
            {
                var parameterType = typeof(Guid);
                var parameterName = "id";
                var action = "GetAccount";
                var xml = "<GetAccount xmlns=\"http://webservice.com\"><id>29dad7b2-7d9c-46aa-b8ed-19386514175e</id></GetAccount>";

                var xmlSerializer = new XmlSerializer(parameterType);

                var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);
                xmlReader.ReadStartElement(action, @namespace);

                var outerXml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);
                var guid = xmlSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(outerXml)));

                guid.ShouldBe(Guid.Parse("29dad7b2-7d9c-46aa-b8ed-19386514175e"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Fact(DisplayName = "Deserialize Guid List")]
        public void OverwriteSoapXml_Guid_List_Test()
        {
            try
            {
                var parameterType = typeof(List<Guid>);
                var parameterName = "ids";
                var action = "GetAccounts";
                var xml = "<GetAccounts xmlns=\"http://webservice.com\"><ids><guid>29dad7b2-7d9c-46aa-b8ed-19386514175e</guid><guid>72bcde9b-dc11-4fea-85d8-5fd6aea99a3d</guid></ids></GetAccounts>";

                var xmlSerializer = new XmlSerializer(parameterType);

                var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);
                xmlReader.ReadStartElement(action, @namespace);

                var outerXml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);
                var guids = xmlSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(outerXml))) as List<Guid>;

                guids.ShouldNotBeNull();
                guids.Count.ShouldBe(2);
                guids.ForEach(g => g.ShouldNotBe(Guid.Empty));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Fact(DisplayName = "Deserialize Account")]
        public void OverwriteSoapXml_Account_Test()
        {
            var parameterType = typeof(Account);
            var parameterName = "account";
            var action = "CreateAccount";
            var xml = "<CreateAccount xmlns=\"http://webservice.com\"><account><Id>29dad7b2-7d9c-46aa-b8ed-19386514175e</Id><Name>123</Name><EMail>234</EMail><Contacts><Contact><Id>1</Id><LastName>345</LastName><FirstName>456</FirstName></Contact><Contact><Id>2</Id><LastName>567</LastName><FirstName>678</FirstName></Contact></Contacts></account></CreateAccount>";

            var xmlSerializer = new XmlSerializer(parameterType);

            var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);
            xmlReader.ReadStartElement(action, @namespace);

            var outerXml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);
            var account = xmlSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(outerXml))) as Account;

            account.ShouldNotBeNull();
            account.Id.ShouldNotBe(Guid.Empty);
            account.Contacts.ShouldNotBeNull();
            account.Contacts.Count.ShouldBe(2);
        }

        [Fact(DisplayName = "Deserialize Multi to Account")]
        public void OverwriteSoapXml_MultiToAccount_Test()
        {
            {
                var parameterType = typeof(Account);
                var parameterName = "account";
                var action = "CreateAccount";
                var xml = "<CreateAccount xmlns=\"http://webservice.com\"><Id>29dad7b2-7d9c-46aa-b8ed-19386514175e</Id><Name>123</Name><EMail>234</EMail><Contacts><Contact><Id>1</Id><LastName>345</LastName><FirstName>456</FirstName></Contact><Contact><Id>2</Id><LastName>567</LastName><FirstName>678</FirstName></Contact></Contacts></CreateAccount>";

                var xmlSerializer = new XmlSerializer(parameterType);

                var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);
                xmlReader.ReadStartElement(action, @namespace);

                var outerXml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);
                var account = xmlSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(outerXml))) as Account;

                account.ShouldNotBeNull();
                account.Id.ShouldNotBe(Guid.Empty);
                account.Contacts.ShouldNotBeNull();
                account.Contacts.Count.ShouldBe(2);
            }
        }

        [Fact(DisplayName = "Deserialize Accounts")]
        public void OverwriteSoapXml_Accounts_Test()
        {
            try
            {
                var parameterType = typeof(List<Account>);
                var parameterName = "accounts";
                var action = "CreateAccounts";
                var xml = "<CreateAccounts xmlns=\"http://webservice.com\"><accounts><Account><Id>29dad7b2-7d9c-46aa-b8ed-19386514175e</Id><Name>123</Name><EMail>234</EMail><Contacts><Contact><Id>1</Id><LastName>345</LastName><FirstName>456</FirstName></Contact><Contact><Id>2</Id><LastName>567</LastName><FirstName>678</FirstName></Contact></Contacts></Account><Account><Id>72bcde9b-dc11-4fea-85d8-5fd6aea99a3d</Id><Name>123</Name><EMail>234</EMail><Contacts><Contact><Id>1</Id><LastName>345</LastName><FirstName>456</FirstName></Contact><Contact><Id>2</Id><LastName>567</LastName><FirstName>678</FirstName></Contact></Contacts></Account></accounts></CreateAccounts>";

                var xmlSerializer = new XmlSerializer(parameterType);

                var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);
                xmlReader.ReadStartElement(action, @namespace);

                var outerXml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);
                var accounts = xmlSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(outerXml))) as List<Account>;

                accounts.ShouldNotBeNull();
                accounts.Count.ShouldBe(2);
                accounts.Last().Id.ShouldNotBe(Guid.Empty);
                accounts.Last().Contacts.ShouldNotBeNull();
                accounts.Last().Contacts.Count.ShouldBe(2);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Fact(DisplayName = "Deserialize string")]
        public void OverwriteSoapXml_String_Test()
        {
            try
            {
                var parameterType = typeof(string);
                var parameterName = "id";
                var action = "GetAccount";
                var xml = "<GetAccount xmlns=\"http://webservice.com\"><id>29dad7b2-7d9c-46aa-b8ed-19386514175e</id></GetAccount>";

                var xmlSerializer = new XmlSerializer(parameterType);

                var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);
                xmlReader.ReadStartElement(action, @namespace);

                var outerXml = SoapXMLHandler.OverwriteSoapXml(parameterType, parameterName, @namespace, xmlReader);
                var guid = xmlSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(outerXml)));

                guid.ShouldBe("29dad7b2-7d9c-46aa-b8ed-19386514175e");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [Fact]
        public void Deserialize_Test()
        {
            var soapAction = "CreateAccount";
            {
                var parameter = typeof(AndroidStructNotification).GetMethod(nameof(AndroidStructNotification.Accout)).GetParameters().First();

                var xml = "<CreateAccount xmlns=\"http://webservice.com\"><id>29dad7b2-7d9c-46aa-b8ed-19386514175e</id></CreateAccount>";
                var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);

                SoapXMLHandler.Deserialize(xmlReader, parameter, soapAction, @namespace)
                    .ShouldBe(Guid.Parse("29dad7b2-7d9c-46aa-b8ed-19386514175e"));
            }

            {
                var parameter = typeof(AndroidStructNotification).GetMethod(nameof(AndroidStructNotification.Accout1)).GetParameters().First();

                var xml = "<CreateAccount xmlns=\"http://webservice.com\"><guid>29dad7b2-7d9c-46aa-b8ed-19386514175e</guid></CreateAccount>";
                var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);

                SoapXMLHandler.Deserialize(xmlReader, parameter, soapAction, @namespace)
                    .ShouldBe(Guid.Parse("29dad7b2-7d9c-46aa-b8ed-19386514175e"));
            }
        }

        [Fact]
        public void Serialize_Test()
        {
            var parameter = typeof(AndroidStructNotification).GetMethod(nameof(AndroidStructNotification.PostAccount)).GetParameters().First();
            var action = "CreateAccount";

            var account = new Account
            {
                Id = new Guid("29dad7b2-7d9c-46aa-b8ed-19386514175e"),
                Name = "123",
                EMail = "234",
                Contacts = new List<Contact>
                {
                    new Contact
                    {
                        Id = 1,
                        FirstName = "456",
                        LastName="345",
                    },
                     new Contact
                    {
                        Id = 2,
                        FirstName = "678",
                        LastName="567",
                    }
                }
            };

            var xml = SoapXMLHandler.Serialize(account, parameter, parameter.ParameterType, action)
                .Replace("Result", string.Empty)
                .Replace("<" + action, $"<{action} xmlns=\"{@namespace}\"");

            //var xml = "<CreateAccount xmlns=\"http://webservice.com\"><account><Id>29dad7b2-7d9c-46aa-b8ed-19386514175e</Id><Name>123</Name><EMail>234</EMail><Contacts><Contact><Id>1</Id><LastName>345</LastName><FirstName>456</FirstName></Contact><Contact><Id>2</Id><LastName>567</LastName><FirstName>678</FirstName></Contact></Contacts></account></CreateAccount>";

            var xmlSerializer = new XmlSerializer(parameter.ParameterType);

            var xmlReader = XmlDictionaryReader.CreateTextReader(Encoding.UTF8.GetBytes(xml), XmlDictionaryReaderQuotas.Max);
            xmlReader.ReadStartElement(action, @namespace);

            var outerXml = SoapXMLHandler.OverwriteSoapXml(parameter.ParameterType, parameter.Name, @namespace, xmlReader);
            var newAccount = xmlSerializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(outerXml))) as Account;

            newAccount.ShouldNotBeNull();
            newAccount.Id.ShouldNotBe(Guid.Empty);
            newAccount.Contacts.ShouldNotBeNull();
            newAccount.Contacts.Count.ShouldBe(2);
        }

        [Fact(DisplayName = "")]
        public void ToLowerCamelCase()
        {
            SoapXMLHandler.ToLowerCamelCase(null)
                .ShouldBeNull();

            SoapXMLHandler.ToLowerCamelCase("haha")
                .ShouldBe("haha");

            SoapXMLHandler.ToLowerCamelCase("Haha")
                .ShouldBe("haha");

            SoapXMLHandler.ToLowerCamelCase("hAha")
                .ShouldBe("hAha");
        }
    }

    public partial class AndroidStructNotification
    {
        /// <remarks/>
        public string _androidToken { get; set; }

        /// <remarks/>
        public int _type { get; set; }

        /// <remarks/>
        public string _alertMessage { get; set; }

        /// <remarks/>
        public long _visitorId { get; set; }

        /// <remarks/>
        public string _visitorGuid { get; set; }

        /// <remarks/>
        public string _partnerId { get; set; }

        public void PostAccount(Account account)
        {
        }

        public void Accout(Guid id)
        {
        }

        public void Accout1([MessageParameterAttribute(Name = "guid")] Guid id)
        {
        }

        public string Detect(string auth, string text)
        {
            return "en";
        }

        public string[] Translate(string auth, string[] texts, string sourceLanguage, string targetLanguage, bool isHtml)
        {
            return new string[] { "1", "2" };
        }

        public void NewPushNotification([System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] AndroidStructNotification[] notifications)
        {
        }
    }
}