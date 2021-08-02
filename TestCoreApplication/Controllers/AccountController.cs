using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace TestCoreApplication.Controllers
{
    [ServiceContract(Namespace = "http://webservice.com", Name = nameof(AccountService))]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [OperationContract]
        [HttpPost]
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
