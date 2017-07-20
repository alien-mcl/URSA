using System.Collections.Generic;
using RDeF.Mapping.Attributes;

namespace URSA.Web.Http.Description.Rdfs
{
    /// <summary>Describes a property.</summary>
    [Class("rdf", "Property")]
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