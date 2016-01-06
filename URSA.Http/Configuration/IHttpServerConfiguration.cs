using System;

namespace URSA.Web.Http.Configuration
{
    /// <summary>Provides a description of the HTTP server.</summary>
    public interface IHttpServerConfiguration
    {
        /// <summary>Gets the base URI.</summary>
        Uri BaseUri { get; }
    }
}