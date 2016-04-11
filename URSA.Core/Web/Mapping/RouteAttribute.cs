using System;

namespace URSA.Web.Mapping
{
    /// <summary>Describes the partial uri associated with the method.</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method)]
    public class RouteAttribute : MappingAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="RouteAttribute" /> class.</summary>
        /// <param name="uri">Part of the uri associated with the method.</param>
        public RouteAttribute(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Length == 0)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            Uri = new Uri((uri[0] == '/' ? String.Empty : "/") + uri, UriKind.Relative);
        }

        /// <summary>Gets the part of the uri associated with the method.</summary>
        public Uri Uri { get; private set; }
    }
}
