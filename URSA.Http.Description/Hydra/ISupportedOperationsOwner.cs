using System.Collections.Generic;
using RDeF.Mapping.Attributes;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes an owner of supported operations.</summary>
    public interface ISupportedOperationsOwner : IResource
    {
        /// <summary>Gets the class supported operations.</summary>
        [Collection("hydra", "supportedOperation")]
        ICollection<IOperation> SupportedOperations { get; }
    }
}