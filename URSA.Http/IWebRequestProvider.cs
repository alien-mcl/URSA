using System;
using System.Net;

namespace URSA.Web.Http
{
    /// <summary>Defines a contract for web request providers.</summary>
    public interface IWebRequestProvider
    {
        /// <summary>Gets the supported protocols.</summary>
        string[] SupportedProtocols { get; }

        /// <summary>Creates the web request.</summary>
        /// <param name="uri">The target Uri of the request.</param>
        /// <returns>Instance of the <paramref name="uri" /> corresponding web request.</returns>
        WebRequest CreateRequest(Uri uri);
    }
}