using System.Collections.Generic;
using RDeF.Mapping.Attributes;
using URSA.Web.Http.Description.Rdfs;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a collection hypermedia.</summary>
    [Class("hydra", "Collection")]
    public interface ICollection : IResource
    {
        /// <summary>Gets the members of this collection.</summary>
        [Collection("hydra", "member")]
        ICollection<IResource> Members { get; }

        /// <summary>Gets or sets a property that this collection is bound to.</summary>
        [Property("hydra", "manages")]
        IProperty Manages { get; set; }

        /// <summary>Gets or sets the number of total items.</summary>
        [Property("hydra", "totalItems")]
        int TotalItems { get; set; }

        /// <summary>Gets or sets the view for this collection.</summary>
        [Property("hydra", "view")]
        IPartialCollectionView View { get; set; }
    }
}