using System.Collections.Generic;
using RDeF.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes an IRI template.</summary>
    [Class("hydra", "IriTemplate")]
    public interface IIriTemplate : IResource
    {
        /// <summary>Gets or sets the IRI template string.</summary>
        [Property("hydra", "template")]
        string Template { get; set; }

        /// <summary>Gets the IRI template mappings.</summary>
        [Collection("hydra", "mapping")]
        ICollection<IIriTemplateMapping> Mappings { get; }
    }
}