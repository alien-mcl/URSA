using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Security
{
    /// <summary>Restricts identities with a given claims an access to a restricted resource.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    public abstract class ClaimBasedSecurityConstraintAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="ClaimBasedSecurityConstraintAttribute"/> class.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        protected ClaimBasedSecurityConstraintAttribute(string claimType, string claimValue = null)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException("claimType");
            }

            ClaimType = claimType;
            ClaimValue = claimValue;
        }

        /// <summary>Gets the type of the claim.</summary>
        public string ClaimType { get; private set; }

        /// <summary>Gets the value of the claim.</summary>
        public string ClaimValue { get; private set; }
    }
}