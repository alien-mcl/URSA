using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Description.Tests.Data
{
    [ExcludeFromCodeCoverage]
    public class Person
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}