using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace URSA.Security
{
    /// <summary>Describes a security requirements of a resource.</summary>
    public class ResourceSecurityInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ResourceSecurityInfo"/> class.</summary>
        public ResourceSecurityInfo() : this(new SecuritySpecificationInfo(), new SecuritySpecificationInfo())
        {
        }

        private ResourceSecurityInfo(SecuritySpecificationInfo allowed, SecuritySpecificationInfo denied)
        {
            Allowed = allowed;
            Denied = denied;
        }

        /// <summary>Gets the allowed claims.</summary>
        public SecuritySpecificationInfo Allowed { get; private set; }

        /// <summary>Gets the denied claims.</summary>
        public SecuritySpecificationInfo Denied { get; private set; }

        /// <summary>Allows the specified claim type.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>The descriptor itself.</returns>
        public ResourceSecurityInfo Allow(string claimType, string claimValue = null)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException("claimType");
            }

            Undeny(claimType, claimValue);
            Allowed.Add(claimType, claimValue);
            return this;
        }

        /// <summary>Disallows the specified claim type.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>The descriptor itself.</returns>
        public ResourceSecurityInfo Disallow(string claimType, string claimValue = null)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException("claimType");
            }

            Allowed.Remove(claimType, claimValue);
            return this;
        }

        /// <summary>Denies the specified claim type.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>The descriptor itself.</returns>
        public ResourceSecurityInfo Deny(string claimType, string claimValue = null)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException("claimType");
            }

            Disallow(claimType, claimValue);
            Denied.Add(claimType, claimValue);
            return this;
        }

        /// <summary>Undenies the specified claim type.</summary>
        /// <param name="claimType">Type of the claim.</param>
        /// <param name="claimValue">The claim value.</param>
        /// <returns>The descriptor itself.</returns>
        public ResourceSecurityInfo Undeny(string claimType, string claimValue = null)
        {
            if (claimType == null)
            {
                throw new ArgumentNullException("claimType");
            }

            Denied.Remove(claimType, claimValue);
            return this;
        }

        /// <summary>Merges specifications overriding settings with those taken from <paramref name="specificSecurityInfo"/>.</summary>
        /// <param name="specificSecurityInfo">The specific security information to merge with.</param>
        /// <returns>Merged specifications.</returns>
        public ResourceSecurityInfo OverrideWith(ResourceSecurityInfo specificSecurityInfo)
        {
            if (specificSecurityInfo == null)
            {
                throw new ArgumentNullException("specificSecurityInfo");
            }

            var result = DeepCopy();
            result.Merge(specificSecurityInfo.Allowed, securitySpecification => securitySpecification.Allow(null, null));
            result.Merge(specificSecurityInfo.Denied, securitySpecification => securitySpecification.Deny(null, null));
            return result;
        }

        /// <summary>Clones this instance.</summary>
        /// <returns>Deeply cloned instance.</returns>
        internal ResourceSecurityInfo DeepCopy()
        {
            return new ResourceSecurityInfo(Allowed.DeepCopy(), Denied.DeepCopy());
        }

        private void Merge(SecuritySpecificationInfo securitySpecificationInfo, Expression<Func<ResourceSecurityInfo, object>> mergingDelegate)
        {
            var method = ((MethodCallExpression)mergingDelegate.Body).Method;
            foreach (var claimType in securitySpecificationInfo)
            {
                if (!securitySpecificationInfo[claimType].Any())
                {
                    method.Invoke(this, new object[] { claimType, null });
                }
                else
                {
                    foreach (var claimValue in securitySpecificationInfo[claimType])
                    {
                        method.Invoke(this, new object[] { claimType, claimValue });
                    }
                }
            }
        }
    }
}