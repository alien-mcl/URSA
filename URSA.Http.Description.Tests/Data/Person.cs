using System;
using System.Collections.Generic;

namespace URSA.Http.Description.Tests.Data
{
    public class Person
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}