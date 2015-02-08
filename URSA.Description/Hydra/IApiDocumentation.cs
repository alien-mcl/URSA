using RomanticWeb.Entities;
using RomanticWeb.Mapping.Attributes;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Describes an API documentation</summary>
    [Class("hydra", "ApiDocumentation")]
    public interface IApiDocumentation : IEntity
    {
        /// <summary>Gets or sets the title of the documentation.</summary>
        [Property("hydra", "title")]
        string Title { get; set; }

        /// <summary>Gets or sets the description of the documentation.</summary>
        [Property("hydra", "description")]
        string Description { get; set; }

        /// <summary>Gets the APIs supported classes.</summary>
        [Collection("hydra", "supportedClasses")]
        ICollection<IClass> SupportedClasses { get; }

        /// <summary>Gets the APIs status codes.</summary>
        [Collection("hydra", "statusCodes")]
        ICollection<int> StatusCodes { get; }

        /// <summary>Gets the APIs entry points.</summary>
        /// <remarks>
        /// In theory, a bone ReST-ful service should have only a single entry point.
        /// Unfortunately, it's not the case in many circumstances and approaches.
        /// URSA will treat each of the methods defined in a controller as an entry point.
        /// </remarks>
        [Collection("hydra", "entrypoint")]
        ICollection<IResource> EntryPoints { get; }
    }
}