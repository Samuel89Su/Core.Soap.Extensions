using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using TestWebService.Models;

namespace TestWebService
{
    /// <summary>
    /// Test 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://webservice.com")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class Test : System.Web.Services.WebService
    {

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod]
        public Account GetAccount(Guid id)
        {
            return new Account
            {
                Id = id,
                Name = "123",
                EMail = "123@1.com",
                Contacts = new List<Contact>
                    {
                        new Contact
                        {
                             Id = 3,
                              FirstName = "Ne",
                               LastName = "fe",
                        }
                    }
            };
        }

        [WebMethod]
        public List<Account> GetAccounts(List<Guid> ids)
        {
            return new List<Account>
            {
                new Account
                {
                    Id = ids.FirstOrDefault(),
                    Name = "123",
                    EMail = "123@1.com",
                    Contacts = new List<Contact>
                        {
                            new Contact
                            {
                                 Id = 3,
                                  FirstName = "Ne",
                                   LastName = "fe",
                            }
                        }
                },
                new Account
                {
                    Id = Guid.NewGuid(),
                    Name = "123",
                    EMail = "123@1.com",
                    Contacts = new List<Contact>
                        {
                            new Contact
                            {
                                 Id = 4,
                                  FirstName = "Ne",
                                   LastName = "fe",
                            }
                        }
                },
            };
        }

        [WebMethod]
        public List<Account> CreateAccounts(List<Account> accounts)
        {
            throw new ArgumentException(nameof(accounts));
        }
    }
}
