using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Rdfs
{
    /// <summary>Describes a class.</summary>
    [Class("rdfs", "Class")]
    public interface IClass : IResource
    {
        /// <summary>Gets the ancestor classes.</summary>
        [Collection("rdfs", "subClassOf")]
        ICollection<IClass> SubClassOf { get; }
    }
}