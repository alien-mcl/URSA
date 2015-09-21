using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Description.Tests.Data
{
    /// <summary>Describes a person.</summary>
    [ExcludeFromCodeCoverage]
    public class Person : IControlledEntity<Guid>
    {
        /// <summary>Gets or sets the person identifier.</summary>
        public Guid Id { get; set; }

        /// <summary>Gets or sets the person firstname.</summary>
        public string FirstName { get; set; }

        /// <summary>Gets or sets the person lastaname.</summary>
        public string LastName { get; set; }

        /// <summary>Gets or sets the person roles.</summary>
        public ICollection<string> Roles { get; set; }
    }
}