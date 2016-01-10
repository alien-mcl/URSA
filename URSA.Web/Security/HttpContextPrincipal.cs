using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Security;
using URSA.Security;

namespace URSA.Web.Security
{
    /// <summary>Provides a basic wrapper for ASP.net HTTP context principal.</summary>
    [ExcludeFromCodeCoverage]
    public class HttpContextPrincipal : IClaimBasedIdentity
    {
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
                if (claimType == null)
                {
                    throw new ArgumentNullException("claimType");
                }

                IEnumerable<string> result = null;
                switch (claimType)
                {
                    case ClaimTypes.Name:
                        result = (!String.IsNullOrEmpty(_principal.Identity.Name) ? new[] { _principal.Identity.Name } : result);
                        break;
                    case ClaimTypes.AuthenticationMethod:
                        result = (!String.IsNullOrEmpty(_principal.Identity.AuthenticationType) ? new[] { _principal.Identity.AuthenticationType } : result);
                        break;
                    default:
                        var formsIdentity = _principal.Identity as FormsIdentity;
                        if (formsIdentity != null)
                        {
                            result = formsIdentity.Claims.Where(claim => claim.Type == claimType).Select(claim => claim.Value);
                        }

                        var windowsIdentity = _principal.Identity as WindowsIdentity;
                        if (windowsIdentity != null)
                        {
                            result = windowsIdentity.Claims.Where(claim => claim.Type == claimType).Select(claim => claim.Value);
                        }

                        var claimsIdentity = _principal.Identity as ClaimsIdentity;
                        if (claimsIdentity != null)
                        {
                            result = claimsIdentity.Claims.Where(claim => claim.Type == claimType).Select(claim => claim.Value);
                        }

                        break;
                }

                return ((result != null) && (result.Any()) ? result : null);
            }
        }
    }
}