using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.Web.Description;

namespace URSA.Security
{
    /// <summary>Provides useful <see cref="SecurableResourceInfo" /> extensions.</summary>
    public static class SecurableResourceInfoExtensions
    {
        /// <summary>Reads the security details from a provided <paramref name="member" />.</summary>
        /// <typeparam name="T">Type of the securable resource.</typeparam>
        /// <param name="securableResource">The securable resource.</param>
        /// <param name="member">The member.</param>
        /// <returns>The securable resource filled with security details.</returns>
        public static T WithSecurityDetailsFrom<T>(this T securableResource, ICustomAttributeProvider member) where T : SecurableResourceInfo
        {
            var customAttributes = (member is MemberInfo ? 
                ((MemberInfo)member).GetCustomAttributes<ClaimBasedSecurityConstraintAttribute>() :
                ((Assembly)member).GetCustomAttributes<ClaimBasedSecurityConstraintAttribute>());
            foreach (var claimSpecification in customAttributes)
            {
                if (claimSpecification is AllowClaimAttribute)
                {
                    securableResource.SecurityRequirements.Allow(claimSpecification.ClaimType, claimSpecification.ClaimValue);
                }
                else
                {
                    securableResource.SecurityRequirements.Deny(claimSpecification.ClaimType, claimSpecification.ClaimValue);
                }
            }

            return securableResource;
        }

        /// <summary>Checks if a given <paramref name="identity" /> is allowed to operate on a given <paramref name="securableResource" />.</summary>
        /// <param name="securableResource">The securable resource to check.</param>
        /// <param name="identity">The identity.</param>
        /// <returns><b>true</b> if a <paramref name="identity" /> meets the <paramref name="securableResource" />'s requirements; otherwise <b>false</b>.</returns>
        public static bool Allows(this SecurableResourceInfo securableResource, IClaimBasedIdentity identity)
        {
            if (securableResource == null)
            {
                throw new ArgumentNullException("securableResource");
            }

            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            var securityRequirements = securableResource.UnifiedSecurityRequirements;
            return (!securityRequirements.Denied.Matches(identity)) && 
                ((!securityRequirements.Allowed.Any()) || (securityRequirements.Allowed.Matches(identity)));
        }

        internal static bool Matches(this SecuritySpecificationInfo securitySpecificationInfo, IClaimBasedIdentity identity)
        {
            foreach (var claimType in securitySpecificationInfo)
            {
                IEnumerable<string> claims;
                if ((claims = identity[claimType]) == null)
                {
                    continue;
                }

                var claimValues = securitySpecificationInfo[claimType];
                var anyValues = claimValues.Any();
                var matchingClaims = from value in claimValues join claim in claims on value equals claim select claim;
                if ((!anyValues) || (matchingClaims.Any()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}