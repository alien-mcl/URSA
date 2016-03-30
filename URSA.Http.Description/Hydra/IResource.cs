using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Represents an <![CDATA[HyDrA]]> resource.</summary>
    [Class("hydra", "Resource")]
    public interface IResource : Rdfs.IResource
    {
        /// <summary>Gets the resource operations.</summary>
        [Collection("hydra", "operation")]
        ICollection<IOperation> Operations { get; }

        /// <summary>Gets the media types supported by given operation.</summary>
        [Collection("ursa", "mediaType")]
        ICollection<string> MediaTypes { get; }
    }
}