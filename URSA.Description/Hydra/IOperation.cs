using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes an operation.</summary>
    [Class("hydra", "Operation")]
    public interface IOperation : IEntity
    {
        /// <summary>Gets the HTTP methods.</summary>
        [Collection("hydra", "method")]
        ICollection<string> Method { get; }

        /// <summary>Gets or sets the class Uri expected in request body.</summary>
        [Property("hydra", "expects")]
        IClass Expects { get; set; }

        /// <summary>Gets or sets the class Uri returned in response body.</summary>
        [Property("hydra", "returns")]
        IClass Returns { get; set; }

        /// <summary>Gets the HTTP status codes.</summary>
        [Property("hydra", "statusCodes")]
        ICollection<int> StatusCodes { get; }
    }
}