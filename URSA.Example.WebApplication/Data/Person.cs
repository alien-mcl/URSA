using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace URSA.Example.WebApplication.Data
{
    public class Person
    {
        [Key]
        public Guid Id { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}