using Microsoft.AspNetCore.Mvc;
using SoapJsonConversion.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace TestCoreApplication.Controllers
{
    [ServiceContract(Namespace = "http://webservice.com")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public AccountController()
        {
        }

        //[OperationContract(Action = "GetAccount")]
        //[HttpPost]
        //public Account GetAccount(Guid id)
        //{
        //    var account = new Account
        //    {
        //        Id = id,
        //        Name = "123",
        //        EMail = "123@1.com",
        //        Contacts = new List<Contact>
        //            {
        //                new Contact
        //                {
        //                     Id = 3,
        //                      FirstName = "Ne",
        //                       LastName = "fe",
        //                }
        //            }
        //    };

        //    return account;
        //}

        [OperationContract(Action = "GetAccountBy")]
        [HttpPost("Account")]
        public async Task<Account> GetAccountAsync(Guid id, string name)
        {
            var account = new Account
            {
                Id = id,
                Name = name,
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

        [OperationContract(Action = "Create")]
        [HttpPost]
        public async Task<Account> Create(Account account)
        {
            account = new Account
            {
                Id = account.Id,
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

        [OperationContract(Action = "CreateAccounts")]
        [HttpPost]
        public async Task<List<Account>> CreateAccounts(List<Account> accounts)
        {
            return accounts.Select(i => new Account
            {
                Id = i.Id,
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

        [OperationContract(Action = "GetAccounts")]
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

        [OperationContract(Action = "Delete")]
        [HttpPost]
        public Guid Delete([FromBody]Guid id)
        {
            return id;
        }
    }
}
