using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Configuration
{
    /// <summary>Provides a basic HTTP configuration.</summary>
    [ExcludeFromCodeCoverage]
    public class StaticHttpServerConfiguration : IHttpServerConfiguration
    {
        /// <summary>Initializes a new instance of the <see cref="StaticHttpServerConfiguration"/> class.</summary>
        /// <param name="baseUri">The base URI.</param>
        public StaticHttpServerConfiguration(Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            BaseUri = baseUri;
        }

        /// <inheritdoc />
        public Uri BaseUri { get; private set; }
    }
}