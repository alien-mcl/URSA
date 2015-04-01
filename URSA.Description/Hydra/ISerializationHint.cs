using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a class serialization hint.</summary>
    public interface ISerializationHint : IEntity
    {
        /// <summary>Gets or sets the media type to be used for serialization.</summary>
        [Property("ursa", "mediaType")]
        string MediaType { get; set; }
    }
}