using System.Collections.Generic;
using RomanticWeb.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Represents a hyperlink.</summary>
    [Class("hydra", "Link")]
    public interface ILink : Rdfs.IProperty, IResource
    {
        /// <summary>Gets the supported operations.</summary>
        [Collection("hydra", "supportedOperation")]
        ICollection<IOperation> SupportedOperations { get; }
    }
}