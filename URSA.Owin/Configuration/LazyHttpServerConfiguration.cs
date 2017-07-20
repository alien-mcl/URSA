using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Configuration
{
    /// <summary>Provides a lazily initialized HTTP configuration.</summary>
    [ExcludeFromCodeCoverage]
    public class LazyHttpServerConfiguration : IHttpServerConfiguration
    {
        /// <inheritdoc />
        public Uri BaseUri { get { return HostingUri; } }

        internal static Uri HostingUri { get; set; }
    }
}