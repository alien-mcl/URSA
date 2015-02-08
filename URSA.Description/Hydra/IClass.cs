using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a class.</summary>
    [Class("hydra", "Class")]
    public interface IClass : IEntity
    {
        /// <summary>Gets the class supported properties.</summary>
        [Collection("hydra", "supportedProperties")]
        ICollection<ISupportedProperty> SupportedProperties { get; }

        /// <summary>Gets the class supported operations.</summary>
        [Collection("hydra", "supportedOperation")]
        ICollection<IOperation> SupportedOperations { get; }

        /// <summary>Gets the ancestor classes.</summary>
        [Collection("rdfs", "subClassOf")]
        ICollection<IClass> SubClassOf { get; }
    }
}