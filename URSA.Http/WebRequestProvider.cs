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
                        result.Headers.Add(Header.AcceptLanguage, entry.Value);
                        break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        WebRequest IWebRequestProvider.CreateRequest(Uri uri, IDictionary<string, string> headers)
        {
            return CreateRequest(uri, headers);
        }
    }
}