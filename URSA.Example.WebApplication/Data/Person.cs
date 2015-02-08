using System;
using System.Collections.Generic;

namespace URSA.Example.WebApplication.Data
{
    public class Person
    {
        public Guid Id { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}