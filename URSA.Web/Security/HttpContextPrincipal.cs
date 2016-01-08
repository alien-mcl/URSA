using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;
using URSA.Security;

namespace URSA.Web.Security
{
    /// <summary>Provides a basic wrapper for ASP.net HTTP context principal.</summary>
    [ExcludeFromCodeCoverage]
    public class HttpContextPrincipal : IClaimBasedIdentity
    {
        private static readonly string[] DefaultClaims = new string[0];

        private readonly IPrincipal _principal;

        /// <summary>Initializes a new instance of the <see cref="HttpContextPrincipal"/> class.</summary>
        /// <param name="principal">The principal.</param>
        public HttpContextPrincipal(IPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException("principal");
            }

            _principal = principal;
        }

        /// <inheritdoc />
        public bool IsAuthenticated { get { return _principal.Identity.IsAuthenticated; } }

        /// <inheritdoc />
        public IEnumerable<string> this[string claimType]
        {
            get
            {
                switch (claimType)
                {
                    case ClaimTypes.Name:
                        return (!String.IsNullOrEmpty(_principal.Identity.Name) ? new[] { _principal.Identity.Name } : DefaultClaims);
                    case ClaimTypes.AuthenticationMethod:
                        return (!String.IsNullOrEmpty(_principal.Identity.AuthenticationType) ? new[] { _principal.Identity.AuthenticationType } : DefaultClaims);
                    default:
                        return DefaultClaims;
                }
            }
        }
    }
}