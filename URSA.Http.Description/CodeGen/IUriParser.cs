using System;
using System.Security.Cryptography.X509Certificates;

namespace URSA.Web.Http.Description.CodeGen
{
    /// <summary>Enumerates all <see cref="IUriParser" /> compatibility types.</summary>
    public enum UriParserCompatibility : int
    {
        /// <summary>Defines a lack of compatibility.</summary>
        None = 0,

        /// <summary>Defines a scheme compatibility.</summary>
        SchemeMatch = 1,

        /// <summary>Defines an exact match compatibility.</summary>
        ExactMatch = Int32.MaxValue
    }

    /// <summary>Provides a basic <see cref="Uri" /> to name-namespace parsing facility contract.</summary>
    public interface IUriParser
    {
        /// <summary>Checks if the parser is applicable for given <paramref name="uri"/>.</summary>
        /// <param name="uri">Uri to check against.</param>
        /// <returns>Compatibility level.</returns>
        UriParserCompatibility IsApplicable(Uri uri);

        /// <summary>Parses the specified URI for name and namespace.</summary>
        /// <param name="uri">The URI to be parsed.</param>
        /// <param name="namespace">The resulting namespace.</param>
        /// <returns>Name of the element.</returns>
        string Parse(Uri uri, out string @namespace);
    }
}