using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace URSA.Web.Http
{
    /// <summary>Provides HTTP web requests.</summary>
    public sealed class WebRequestProvider : IWebRequestProvider
    {
        private static readonly string[] Protocols = new[] { "http", "https" };

        /// <inheritdoc />
        public string[] SupportedProtocols { get { return Protocols; } }

        /// <summary>Creates the HTTP request.</summary>
        /// <param name="uri">The target Uri of the request.</param>
        /// <returns>Instance of the <see cref="HttpWebRequest" />.</returns>
        public HttpWebRequest CreateRequest(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (!SupportedProtocols.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            var result = (HttpWebRequest)WebRequest.Create(uri);
            return result;
        }

        /// <inheritdoc />
        WebRequest IWebRequestProvider.CreateRequest(Uri uri)
        {
            return CreateRequest(uri);
        }
    }
}