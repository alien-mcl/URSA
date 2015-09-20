using System;
using System.Collections.Generic;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{    
    /// <summary>Provides a description of the facility for type description builders.</summary>
    public interface ITypeDescriptionBuilder
    {
        /// <summary>Gets the supported media type profiles.</summary>
        IEnumerable<Uri> SupportedProfiles { get; }

        /// <summary>Builds the type description.</summary>
        /// <param name="context">The context.</param>
        /// <returns>Context's type description</returns>
        IResource BuildTypeDescription(DescriptionContext context);

        /// <summary>Builds the type description.</summary>
        /// <param name="context">The context.</param>
        /// <param name="requiresRdf">Flag determining whether the context's type requires an RDF approach.</param>
        /// <returns>Context's type description</returns>
        IResource BuildTypeDescription(DescriptionContext context, out bool requiresRdf);
    }
}