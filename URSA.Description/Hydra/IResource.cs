using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Represents an resource.</summary>
    [Class("hydra", "Resource")]
    public interface IResource : IEntity
    {
        /// <summary>Gets the resource operations.</summary>
        [Collection("hydra", "operation")]
        ICollection<IOperation> Operations { get; }
    }
}