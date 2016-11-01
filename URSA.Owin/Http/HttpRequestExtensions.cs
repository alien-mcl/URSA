#if CORE
using System;
using Microsoft.AspNetCore.Http;

namespace URSA.AspNetCore.Http
{
    /// <summary>Provides useful extension methods for <see cref="HttpRequest" /> class.</summary>
    public static class HttpRequestExtensions
    {
        /// <summary>Retrieves a request's absolute Url.</summary>
        /// <param name="request">Request from which to obtain a Url.</param>
        /// <returns>Url string of the given request.</returns>
        public static string ToUrlString(this HttpRequest request)
        {
            return String.Format("{0}://{1}{2}{3}", request.Scheme, request.Host, request.Path, request.QueryString);
        }
    }
}
#endif