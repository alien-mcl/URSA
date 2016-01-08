using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Security
{
    /// <summary>Denies identities with a given claims an access to a restricted resource.</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
    [ExcludeFromCodeCoverage]
    public sealed class DenyClaimAttribute : ClaimBasedSecurityConstraintAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="DenyClaimAttribute"/> class.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        public DenyClaimAttribute(string claimType, string claimValue = null) : base(claimType, claimValue)
        {
        }
    }
}