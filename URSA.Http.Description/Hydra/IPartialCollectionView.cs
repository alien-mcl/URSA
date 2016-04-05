using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes a partial collection view.</summary>
    [Class("hydra", "PartialCollectionView")]
    public interface IPartialCollectionView : IResource
    {
        /// <summary>Gets or sets a link to the next page.</summary>
        [Property("hydra", "next")]
        IEntity Next { get; set; }

        /// <summary>Gets or sets a link to the previous page.</summary>
        [Property("hydra", "previous")]
        IEntity Previous { get; set; }

        /// <summary>Gets or sets a link to the first page.</summary>
        [Property("hydra", "first")]
        IEntity First { get; set; }

        /// <summary>Gets or sets a link to the last page.</summary>
        [Property("hydra", "last")]
        IEntity Last { get; set; }

        /// <summary>Gets or sets the number of items per single page.</summary>
        [Property("hydra", "itemsPerPage")]
        int ItemsPerPage { get; set; }
    }
}