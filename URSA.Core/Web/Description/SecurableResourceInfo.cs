using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Security;
using URSA.Web.Http;

namespace URSA.Web.Description
{
    /// <summary>Describes a resource being a subject to security constraints.</summary>
    public abstract class SecurableResourceInfo
    {
        private ResourceSecurityInfo _unifiedResourceSecurityInfo;

        /// <summary>Initializes a new instance of the <see cref="SecurableResourceInfo"/> class.</summary>
        /// <param name="url">The URL of the resource.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        protected internal SecurableResourceInfo(Url url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            Url = url;
            SecurityRequirements = new ResourceSecurityInfo();
        }

        /// <summary>Gets the base URL of the method without arguments.</summary>
        public Url Url { get; private set; }

        /// <summary>Gets the security requirements.</summary>
        public ResourceSecurityInfo SecurityRequirements { get; private set; }

        /// <summary>Gets the unified with it's <see cref="SecurableResourceInfo.Owner" />'s security requirements.</summary>
        public ResourceSecurityInfo UnifiedSecurityRequirements
        {
            get
            {
                if (_unifiedResourceSecurityInfo != null)
                {
                    return _unifiedResourceSecurityInfo;
                }

                var current = this;
                _unifiedResourceSecurityInfo = current.SecurityRequirements;
                while (current.Owner != null)
                {
                    _unifiedResourceSecurityInfo = current.Owner.SecurityRequirements.OverrideWith(_unifiedResourceSecurityInfo);
                    current = current.Owner;
                }

                return _unifiedResourceSecurityInfo;
            }
        }

        /// <summary>Gets the owner of this securable resource.</summary>
        public abstract SecurableResourceInfo Owner { get; }
    }
}