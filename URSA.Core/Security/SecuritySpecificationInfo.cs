using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Security
{
    /// <summary>Provides a claims specification of a resource.</summary>
    public class SecuritySpecificationInfo : IEnumerable<string>
    {
        private readonly IDictionary<string, IDictionary<string, string>> _claimValues;

        internal SecuritySpecificationInfo()
        {
            _claimValues = new ConcurrentDictionary<string, IDictionary<string, string>>();
        }

        /// <summary>Gets the values of a specified <paramref name="claimType" />.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <returns>Values for a given <paramref name="claimType" />.</returns>
        public IEnumerable<string> this[string claimType]
        {
            get
            {
                if (claimType == null)
                {
                    throw new ArgumentNullException("claimType");
                }

                IDictionary<string, string> claimValues;
                return (_claimValues.TryGetValue(claimType, out claimValues) ? claimValues.Keys : null);
            }
        }

        /// <inheritdoc />
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            return _claimValues.Keys.GetEnumerator();
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)this).GetEnumerator();
        }

        internal void Add(string claimType, string claimValue = null)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException("claimType");
            }

            IDictionary<string, string> claimValues;
            if (!_claimValues.TryGetValue(claimType, out claimValues))
            {
                _claimValues[claimType] = claimValues = new ConcurrentDictionary<string, string>();
            }

            if (claimValue == null)
            {
                claimValues.Clear();
            }
            else
            {
                claimValues[claimValue] = null;
            }
        }

        internal void Remove(string claimType, string claimValue = null)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException("claimType");
            }

            IDictionary<string, string> claimValues;
            if (!_claimValues.TryGetValue(claimType, out claimValues))
            {
                return;
            }

            if (claimValue == null)
            {
                _claimValues.Remove(claimType);
            }
            else
            {
                claimValues.Remove(claimValue);
                if (claimValues.Count == 0)
                {
                    _claimValues.Remove(claimType);
                }
            }
        }

        internal SecuritySpecificationInfo DeepCopy()
        {
            SecuritySpecificationInfo result = new SecuritySpecificationInfo();
            foreach (var claimSpecification in _claimValues)
            {
                var clonedClaimSpecification = new ConcurrentDictionary<string, string>();
                foreach (var claimValue in claimSpecification.Value)
                {
                    clonedClaimSpecification[claimValue.Key] = null;
                }

                result._claimValues[claimSpecification.Key] = clonedClaimSpecification;
            }

            return result;
        }
    }
}