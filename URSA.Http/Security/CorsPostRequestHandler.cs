using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URSA.Web.Http.Security
{
    /// <summary>Provides a default implementation of the Cross-Origin Resource Sharing mechanism.</summary>
    public class CorsPostRequestHandler : IPostRequestHandler
    {
        /// <summary>Defines an origin wildcard match.</summary>
        public const string Any = "*";

        /// <summary>Defines a collection of enabled elements to "*" (any).</summary>
        public static readonly string[] WithAny = { Any };

        private const string AnyHeaders = "Content-Type, Content-Length, Accept, Accept-Language, Accept-Charser, Accept-Encoding, Accept-Ranges, Authorization, X-Auth-Token, X-Requested-With";
        private readonly bool _allowAnyOrigin;
        private readonly bool _exposeAnyHeader;
        private readonly IEnumerable<string> _allowedOrigins;
        private readonly string _allowedHeaders;
        private readonly string _exposedHeaders;

        /// <summary>Initializes a new instance of the <see cref="CorsPostRequestHandler"/> class.</summary>
        [ExcludeFromCodeCoverage]
        public CorsPostRequestHandler() : this(WithAny, WithAny, WithAny)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CorsPostRequestHandler"/> class.</summary>
        /// <param name="allowedOrigins">Allowed origins.</param>
        [ExcludeFromCodeCoverage]
        public CorsPostRequestHandler(IEnumerable<string> allowedOrigins) : this(allowedOrigins, WithAny, WithAny)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CorsPostRequestHandler"/> class.</summary>
        /// <param name="allowedOrigins">Allowed origins.</param>
        /// <param name="allowedHeaders">Allowed headers.</param>
        [ExcludeFromCodeCoverage]
        public CorsPostRequestHandler(IEnumerable<string> allowedOrigins, IEnumerable<string> allowedHeaders) : this(allowedOrigins, allowedHeaders, WithAny)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="CorsPostRequestHandler"/> class.</summary>
        /// <param name="allowedOrigins">Allowed origins.</param>
        /// <param name="allowedHeaders">Allowed headers.</param>
        /// <param name="exposedHeaders">Exposed headers.</param>
        [ExcludeFromCodeCoverage]
        public CorsPostRequestHandler(IEnumerable<string> allowedOrigins, IEnumerable<string> allowedHeaders, IEnumerable<string> exposedHeaders)
        {
            if (allowedOrigins == null)
            {
                throw new ArgumentNullException("allowedOrigins");
            }

            if (!allowedOrigins.Any())
            {
                throw new ArgumentOutOfRangeException("allowedOrigins");
            }

            if (allowedHeaders == null)
            {
                throw new ArgumentNullException("allowedHeaders");
            }

            if (!allowedHeaders.Any())
            {
                throw new ArgumentOutOfRangeException("allowedHeaders");
            }

            if (exposedHeaders == null)
            {
                throw new ArgumentNullException("exposedHeaders");
            }

            if (!exposedHeaders.Any())
            {
                throw new ArgumentOutOfRangeException("exposedHeaders");
            }

            _allowAnyOrigin = ((_allowedOrigins = allowedOrigins).Any(allowedOrigin => allowedOrigin == Any));
            _allowedHeaders = InitializeAllowedHeaders(allowedHeaders);
            _exposedHeaders = InitializeExposedHeaders(exposedHeaders, out _exposeAnyHeader);
        }

        /// <inheritdoc />
        public Task Process(IResponseInfo responseInfo)
        {
            if (responseInfo == null)
            {
                throw new ArgumentNullException("responseInfo");
            }

            var response = responseInfo as ResponseInfo;
            if (response == null)
            {
                throw new ArgumentOutOfRangeException("responseInfo");
            }

            string matchingOrigin;
            if ((String.IsNullOrEmpty(response.Request.Headers.Origin)) || ((matchingOrigin = IsOriginAllowed(response.Request.Headers.Origin)) == null))
            {
                return Task.FromResult(0);
            }

            response.Headers.AccessControlAllowOrigin = matchingOrigin;
            response.Headers.AccessControlExposeHeaders = (!_exposeAnyHeader ? _exposedHeaders :
                String.Join(", ", ((IEnumerable<Header>)response.Request.Headers).Select(header => header.Name)));
            response.Headers.AccessControlAllowHeaders = _allowedHeaders;
            return Task.FromResult(0);
        }

        private static string InitializeAllowedHeaders(IEnumerable<string> allowedHeaders)
        {
            var allowed = new StringBuilder(64);
            foreach (var allowedHeader in allowedHeaders)
            {
                if (String.IsNullOrEmpty(allowedHeader))
                {
                    continue;
                }

                if (allowedHeader == Any)
                {
                    return AnyHeaders;
                }

                allowed.AppendFormat("{0}, ", allowedHeader);
            }

            if (allowed.Length > 2)
            {
                allowed.Remove(allowed.Length - 2, 2);
            }

            return allowed.ToString();
        }

        private static string InitializeExposedHeaders(IEnumerable<string> exposedHeaders, out bool exposeAnyHeader)
        {
            exposeAnyHeader = false;
            var exposed = new StringBuilder(64);
            foreach (var exposedHeader in exposedHeaders)
            {
                if (String.IsNullOrEmpty(exposedHeader))
                {
                    continue;
                }

                if (exposedHeader == Any)
                {
                    exposeAnyHeader = true;
                    exposed.Clear();
                    break;
                }

                exposed.AppendFormat("{0}, ", exposedHeader);
            }

            if (exposed.Length > 2)
            {
                exposed.Remove(exposed.Length - 2, 2);
            }

            return exposed.ToString();
        }

        private string IsOriginAllowed(string origin)
        {
            return (_allowAnyOrigin ? "*" : _allowedOrigins.FirstOrDefault(allowedOrigin => String.Compare(allowedOrigin, origin, true) == 0));
        }
    }
}