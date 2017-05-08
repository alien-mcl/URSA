using System.Collections.Generic;
using RDeF.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a class.</summary>
    [Class("hydra", "Class")]
    public interface IClass : Rdfs.IClass, ISupportedOperationsOwner
    {
        /// <summary>Gets the class supported properties.</summary>
        [Collection("hydra", "supportedProperty")]
        ICollection<ISupportedProperty> SupportedProperties { get; }
    }
}