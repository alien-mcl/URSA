using System.Collections.Generic;

namespace URSA.Security
{
    /// <summary>Provides a basic description of a claim-based identity.</summary>
    public interface IClaimBasedIdentity
    {
        /// <summary>Gets a value indicating whether this instance is authenticated.</summary>
        bool IsAuthenticated { get; }

        /// <summary>Gets the value of a given <paramref name="claimType" />.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <returns>Values of the claim if set; otherwise <b>null</b>.</returns>
        IEnumerable<string> this[string claimType] { get; }
    }
}