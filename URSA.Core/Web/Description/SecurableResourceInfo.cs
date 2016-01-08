using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Security;

namespace URSA.Web.Description
{
    /// <summary>Describes a resource being a subject to security constraints.</summary>
    [ExcludeFromCodeCoverage]
    public abstract class SecurableResourceInfo
    {
        /// <summary>Initializes a new instance of the <see cref="SecurableResourceInfo"/> class.</summary>
        /// <param name="uri">The URI of the resource.</param>
        protected SecurableResourceInfo(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            Uri = uri;
            SecurityRequirements = new ResourceSecurityInfo();
        }

        /// <summary>Gets the base uri of the method without arguments.</summary>
        public Uri Uri { get; private set; }

        /// <summary>Gets the security requirements.</summary>
        public ResourceSecurityInfo SecurityRequirements { get; private set; }

        /// <summary>Gets the unified with it's <see cref="SecurableResourceInfo.Owner" />'s security requirements.</summary>
        public ResourceSecurityInfo UnifiedSecurityRequirements
        {
            get
            {
                var current = this;
                ResourceSecurityInfo result = current.SecurityRequirements;
                while (current.Owner != null)
                {
                    result = current.Owner.SecurityRequirements.OverrideWith(result);
                    current = current.Owner;
                }

                return result;
            }
        }

        /// <summary>Gets the owner of this securable resource.</summary>
        public abstract SecurableResourceInfo Owner { get; }
    }
}