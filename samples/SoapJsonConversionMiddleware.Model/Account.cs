using System;
using System.Collections.Generic;

namespace SoapJsonConversion.Model
{
    public interface IAccount
    {
        List<Contact> Contacts { get; set; }
        string EMail { get; set; }
        Guid Id { get; set; }
        string Name { get; set; }
    }

    public class Account : IAccount
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EMail { get; set; }

        public List<Contact> Contacts { get; set; }
    }



    public class Contact
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }
}
