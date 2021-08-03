using CustomMiddleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TestCoreApplication.Controllers
{
    [ServiceContract(Namespace = "http://webservice.com")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [OperationContract(Action = "GetAccount")]
        [HttpPost]
        public Account GetAccount(Guid id)
        {
            var account = new Account
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

            return account;
        }

        [OperationContract]
        [HttpPost]
        public async Task<List<Account>> GetAccounts(List<Guid> ids)
        {
            return ids.Select(i => new Account
            {
                Id = i,
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
            })
                .ToList();
        }
    }
}
