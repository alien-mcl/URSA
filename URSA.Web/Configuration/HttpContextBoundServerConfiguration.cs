using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using URSA.Web.Http.Configuration;

namespace URSA.Web.Configuration
{
    /// <summary>Provides a <see cref="HttpContext" /> based configuration.</summary>
    [ExcludeFromCodeCoverage]
    public class HttpContextBoundServerConfiguration : IHttpServerConfiguration
    {
        private Uri _baseUri = null;

        /// <inheritdoc />
        public Uri BaseUri
        {
            get
            {
                if ((_baseUri == null) && (HttpContext.Current != null))
                {
                    _baseUri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
                }

                return _baseUri;
            }
        }
    }
}