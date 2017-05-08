using System;
using System.Collections.Generic;
using System.Reflection;
using RDeF.Entities;
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
        IClass BuildTypeDescription(DescriptionContext context);

        /// <summary>Builds the type description.</summary>
        /// <param name="context">The context.</param>
        /// <param name="requiresRdf">Flag determining whether the context's type requires an RDF approach.</param>
        /// <returns>Context's type description</returns>
        IClass BuildTypeDescription(DescriptionContext context, out bool requiresRdf);

        /// <summary>Gets the supported property identifier.</summary>
        /// <param name="property">The property.</param>
        /// <param name="declaringType">Optional type declaring the property overriding property's settings.</param>
        /// <returns><see cref="Iri" /> for a given <paramref name="property" />.</returns>
        Iri GetSupportedPropertyId(PropertyInfo property, Type declaringType = null);

        /// <summary>Creates a sub-class of the given <paramref name="class" />.</summary>
        /// <param name="context">The context.</param>
        /// <param name="class">The class to be sub-classed.</param>
        /// <param name="contextTypeOverride">The type that should override the one stored in <paramref name="context" />.</param>
        /// <returns>Sub-classed instance of the given <paramref name="class" />.</returns>
        IClass SubClass(DescriptionContext context, IClass @class, Type contextTypeOverride = null);
    }
}