using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Security
{
    /// <summary>Provides a basic implementation of the <see cref="IClaimBasedIdentity" />.</summary>
    [ExcludeFromCodeCoverage]
    public class BasicClaimBasedIdentity : IClaimBasedIdentity
    {
        private readonly IDictionary<string, IEnumerable<string>> _claims = new ConcurrentDictionary<string, IEnumerable<string>>();

        /// <inheritdoc />
        public bool IsAuthenticated { get { return false; } }

        /// <inheritdoc />
        public IEnumerable<string> this[string claimType]
        {
            get
            {
                if (claimType == null)
                {
                    throw new ArgumentNullException("claimType");
                }

                IEnumerable<string> result;
                return (_claims.TryGetValue(claimType, out result) ? result : null);
            }
        }
    }
}