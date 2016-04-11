using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace URSA.Security
{
    /// <summary>Provides a basic implementation of the <see cref="IClaimBasedIdentity" />.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
    public class BasicClaimBasedIdentity : IClaimBasedIdentity
    {
        private static readonly string[] Anonymous = { "anonymous" };

        private readonly IEnumerable<string> _userName;

        /// <summary>Initializes a new instance of the <see cref="BasicClaimBasedIdentity"/> class.</summary>
        public BasicClaimBasedIdentity()
        {
            _userName = null;
        }

        /// <summary>Initializes a new instance of the <see cref="BasicClaimBasedIdentity"/> class.</summary>
        /// <param name="userName">Name of the user if the identity should be authenticated.</param>
        public BasicClaimBasedIdentity(string userName)
        {
            _userName = (!String.IsNullOrEmpty(userName) ? new[] { userName } : null);
        }

        /// <inheritdoc />
        public bool IsAuthenticated { get { return _userName != null; } }

        /// <inheritdoc />
        public IEnumerable<string> this[string claimType]
        {
            get
            {
                if (claimType == null)
                {
                    throw new ArgumentNullException("claimType");
                }

                switch (claimType)
                {
                    case ClaimTypes.Anonymous:
                        return (_userName != null ? null : Anonymous);
                    case ClaimTypes.Name:
                        return _userName;
                    default:
                        return null;
                }
            }
        }
    }
}