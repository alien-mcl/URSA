using System;
using System.Diagnostics.CodeAnalysis;
using RomanticWeb;
using RomanticWeb.Entities;

namespace URSA.Web.Http.Tests.Data
{
    [ExcludeFromCodeCoverage]
    public class Person : IControlledEntity<int>
    {
        public int Key { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string[] Roles { get; set; }
    }
}