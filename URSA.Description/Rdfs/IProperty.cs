using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Rdfs
{
    /// <summary>Describes a property.</summary>
    [Class("rdfs", "Property")]
    public interface IProperty : IResource
    {
        /// <summary>Gets the range.</summary>
        [Collection("rdfs", "range")]
        ICollection<IResource> Range { get; }

        /// <summary>Gets the domain.</summary>
        [Collection("rdfs", "domain")]
        ICollection<IClass> Domain { get; }
    }
}