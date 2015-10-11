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
        Rdfs.IProperty Property { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is required in the request body.</summary>
        [Property("hydra", "required")]
        bool Required { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is readable.</summary>
        [Property("hydra", "readable")]
        bool Readable { get; set; }

        /// <summary>Gets or sets a value indicating whether the property is writeable.</summary>
        [Property("hydra", "writeable")]
        bool Writeable { get; set; }
    }
}