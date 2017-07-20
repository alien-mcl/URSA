using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using URSA.Security;

namespace URSA.Owin.Security
{
    /// <summary>Provides a basic OWIN based wrapper for URSA claim based security model.</summary>
    [ExcludeFromCodeCoverage]
    public class OwinPrincipal : IClaimBasedIdentity
    {
        private readonly ClaimsPrincipal _principal;

        /// <summary>Initializes a new instance of the <see cref="OwinPrincipal"/> class. </summary>
        /// <param name="principal">The principal.</param>
        public OwinPrincipal(ClaimsPrincipal principal)
        {
            _principal = principal ?? new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Anonymous, "anonymous") }));
        }

        /// <inheritdoc />
        public bool IsAuthenticated { get { return (_principal.Identity.IsAuthenticated) || (this[ClaimTypes.Anonymous].Any()); } }

        /// <inheritdoc />
        public IEnumerable<string> this[string claimType]
        {
            get
            {
                if (claimType == null)
                {
                    throw new ArgumentNullException("claimType");
                }

                var result = _principal.Claims.Where(claim => claim.Type == claimType).Select(claim => claim.Value);
                return (result.Any() ? result : null);
            }
        }
    }
}