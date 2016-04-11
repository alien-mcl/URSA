using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Security
{
    /// <summary>Allows identities with a given claims an access to a restricted resource.</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    public sealed class AllowClaimAttribute : ClaimBasedSecurityConstraintAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="AllowClaimAttribute"/> class.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        public AllowClaimAttribute(string claimType, string claimValue = null) : base(claimType, claimValue)
        {
        }
    }
}