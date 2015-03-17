using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http
{
    /// <summary>Acts as a base class for HTTP client proxies.</summary>
    public class Client
    {
        /// <summary>Initializes a new instance of the <see cref="Client"/> class.</summary>
        /// <param name="baseUri">The base URI.</param>
        [ExcludeFromCodeCoverage]
        public Client(Uri baseUri)
        {
            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            BaseUri = baseUri;
        }

        private Uri BaseUri { get; set; }

        /// <summary>Calls the ReST service using specified HTTP verb.</summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="url">Relative URL of the service to call.</param>
        /// <param name="uriArguments">The URI template arguments.</param>
        /// <param name="bodyArguments">The body arguments.</param>
        /// <returns>Result of the call.</returns>
        protected object Call(Verb verb, string url, dynamic uriArguments, params object[] bodyArguments)
        {
            return null;
        }
    }
}