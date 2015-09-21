using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using URSA.Web;

namespace URSA.Example.WebApplication.Data
{
    /// <summary>Describes a person.</summary>
    public class Person : IControlledEntity<Guid>
    {
        /// <summary>Gets or sets the person's identifier.</summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary> Gets or sets the person's firstname.</summary>
        public string Firstname { get; set; }

        /// <summary> Gets or sets the person's lastname.</summary>
        public string Lastname { get; set; }

        /// <summary> Gets or sets the person's roles.</summary>
        public ICollection<string> Roles { get; set; }
    }
}