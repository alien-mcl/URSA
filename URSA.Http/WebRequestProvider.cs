using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace URSA.Web.Http
{
    /// <summary>Provides HTTP web requests.</summary>
    public sealed class WebRequestProvider : IWebRequestProvider
    {
        private static readonly string[] Protocols = { "http", "https" };

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public string[] SupportedProtocols { get { return Protocols; } }

        /// <summary>Creates the HTTP request.</summary>
        /// <param name="uri">The target Uri of the request.</param>
        /// <param name="headers">Headers of the request.</param>
        /// <returns>Instance of the <see cref="HttpWebRequest" />.</returns>
        public HttpWebRequest CreateRequest(Uri uri, IDictionary<string, string> headers)
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
            foreach (var entry in headers)
            {
                switch (entry.Key)
                {
                    case Header.Accept:
                        result.Accept = entry.Value;
                        break;
                    default:
                        result.Headers[entry.Key] = entry.Value;
                        break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        WebRequest IWebRequestProvider.CreateRequest(Uri uri, IDictionary<string, string> headers)
        {
            return CreateRequest(uri, headers);
        }
    }
}