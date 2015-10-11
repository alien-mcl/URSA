using RomanticWeb.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes an IRI template mapping.</summary>
    [Class("hydra", "IriTemplateMapping")]
    public interface IIriTemplateMapping : IResource
    {
        /// <summary>Gets or sets the variable name.</summary>
        [Property("hydra", "variable")]
        string Variable { get; set; }

        /// <summary>Gets or sets the target property of the mapping.</summary>
        [Property("hydra", "property")]
        Rdfs.IProperty Property { get; set; }

        /// <summary>Gets or sets a value indicating whether the mapping is required.</summary>
        [Property("hydra", "required")]
        bool Required { get; set; }
    }
}