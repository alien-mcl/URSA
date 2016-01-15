using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using URSA.Security;

namespace URSA.Web.Http.Security
{
    /// <summary>Provides an implementation of the Basic authentication scheme.</summary>
    public class BasicAuthenticationProvider : IAuthenticationProvider, IDefaultAuthenticationScheme
    {
        /// <summary>Defines the name of this authentication type.</summary>
        public const string AuthenticationScheme = "Basic";

        /// <summary>Defines a custom derivative of an 'WWW-Authenticate' response header to prevent browsers challenge users on AJAX originated requests.</summary>
        public const string XWWWAuthenticate = "X-WWW-Authenticate";

        /// <summary>Defines a common AJAX value for the 'X-Requested-With' header.</summary>
        public const string XMLHttpRequest = "XMLHttpRequest";

        private readonly IIdentityProvider _identityProvider;

        /// <summary>Initializes a new instance of the <see cref="BasicAuthenticationProvider"/> class.</summary>
        /// <param name="identityProvider">The authentication provider.</param>
        [ExcludeFromCodeCoverage]
        public BasicAuthenticationProvider(IIdentityProvider identityProvider)
        {
            if (identityProvider == null)
            {
                throw new ArgumentNullException("identityProvider");
            }

            _identityProvider = identityProvider;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public string Scheme { get { return AuthenticationScheme; } }

        /// <inheritdoc />
        public async Task Process(IRequestInfo request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var httpRequest = request as RequestInfo;
            if (httpRequest == null)
            {
                throw new ArgumentOutOfRangeException("request");
            }

            await AuthenticateInternal(httpRequest);
        }

        /// <inheritdoc />
        public async Task Process(IResponseInfo response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var httpResponse = response as ResponseInfo;
            if (httpResponse == null)
            {
                throw new ArgumentOutOfRangeException("response");
            }

            await Challenge(httpResponse);
        }

        private static Task Challenge(ResponseInfo response)
        {
            var challengeScheme = (response.Request.Headers.XRequestedWith != XMLHttpRequest ? Header.WWWAuthenticate : XWWWAuthenticate);
            response.Headers.Add(challengeScheme, AuthenticationScheme);
            var accessControlExposeHeaders = response.Headers.AccessControlExposeHeaders ?? String.Empty;
            if (!String.IsNullOrEmpty(response.Request.Headers.Origin))
            {
                response.Headers.AccessControlExposeHeaders = accessControlExposeHeaders + (accessControlExposeHeaders.Length > 0 ? ", " : String.Empty) + challengeScheme;
            }

            response.Status = HttpStatusCode.Unauthorized;
            return Task.FromResult(0);
        }

        private static bool ParseAuthorizationHeader(string authorizationString, out string userName, out string password)
        {
            userName = null;
            password = null;
            string base64Credentials = authorizationString.Substring(6);
            string[] credentials;
            try
            {
                credentials = Encoding.ASCII.GetString(Convert.FromBase64String(base64Credentials)).Split(':');
            }
            catch
            {
                return false;
            }

            if ((credentials.Length != 2) || (String.IsNullOrEmpty(credentials[0])) || (String.IsNullOrEmpty(credentials[1])))
            {
                return false;
            }

            userName = credentials[0];
            password = credentials[1];
            return true;
        }

        private Task AuthenticateInternal(RequestInfo request)
        {
            var authorization = request.Headers.Authorization;
            if ((String.IsNullOrEmpty(authorization)) || (!authorization.StartsWith(AuthenticationScheme)))
            {
                return Task.FromResult(0);
            }

            string userName;
            string password;
            if (!ParseAuthorizationHeader(authorization, out userName, out password))
            {
                return Task.FromResult(0);
            }

            var identity = _identityProvider.ValidateCredentials(userName, password);
            if (identity != null)
            {
                request.Identity = identity;
            }

            return Task.FromResult(0);
        }
    }
}