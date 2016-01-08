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
    }
}