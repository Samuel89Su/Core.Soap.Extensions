﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestWebService.Models
{
    public class Account
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