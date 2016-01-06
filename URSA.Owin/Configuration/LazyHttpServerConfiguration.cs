using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Configuration
{
    /// <summary>Provides a lazily initialized HTTP configuration.</summary>
    [ExcludeFromCodeCoverage]
    public class LazyHttpServerConfiguration : IHttpServerConfiguration
    {
        internal static Uri HostingUri { get; set; }

        /// <inheritdoc />
        public Uri BaseUri { get { return HostingUri; } }
    }
}