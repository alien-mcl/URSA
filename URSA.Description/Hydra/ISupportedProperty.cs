using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a supported property.</summary>
    [Class("hydra", "SupportedProperty")]
    public interface ISupportedProperty : IEntity
    {
        /// <summary>Gets or sets an RDF property description.</summary>
        [Property("hydra", "property")]
        IProperty Property { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is required in the request body.</summary>
        [Property("hydra", "required")]
        bool Required { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is read-only.</summary>
        [Property("hydra", "readonly")]
        bool ReadOnly { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is write-only.</summary>
        [Property("hydra", "writeonly")]
        bool WriteOnly { get; set; }
    }
}