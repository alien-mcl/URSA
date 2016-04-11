using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Security;

namespace URSA.Web.Description
{
    /// <summary>Describes a resource being a subject to security constraints.</summary>
    public abstract class SecurableResourceInfo
    {
        private ResourceSecurityInfo _unifiedResourceSecurityInfo;

        /// <summary>Initializes a new instance of the <see cref="SecurableResourceInfo"/> class.</summary>
        /// <param name="uri">The URI of the resource.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        protected internal SecurableResourceInfo(Uri uri)
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