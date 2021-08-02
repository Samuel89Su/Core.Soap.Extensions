using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace TestCoreApplication
{
    [ServiceContract(Namespace = "http://webservice.com", Name = nameof(AccountService))]
    public class AccountService //: IAccountService
    {
        [OperationContract]
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
        public List<Account> GetAccounts(Ids ids)
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

    //[ServiceContract(Namespace = "http://webservice.com", Name = nameof(AccountService))]
    //public interface IAccountService
    //{
    //    [OperationContract]
    //    Account GetAccount(Guid id);
    //    [OperationContract]
    //    List<Account> GetAccounts(Ids ids);
    //}

    public class Ids : List<Guid>
    { }
}
