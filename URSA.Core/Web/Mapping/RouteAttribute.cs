using System;
using URSA.Web.Http;

namespace URSA.Web.Mapping
{
    /// <summary>Describes the partial URL associated with the method.</summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Method)]
    public class RouteAttribute : MappingAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="RouteAttribute" /> class.</summary>
        /// <param name="url">Part of the URL associated with the method.</param>
        public RouteAttribute(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            if (url.Length == 0)
            {
                throw new ArgumentOutOfRangeException("url");
            }

            Url = UrlParser.Parse((url[0] == '/' ? String.Empty : "/") + url);
        }

        /// <summary>Gets the part of the URL associated with the method.</summary>
        public Url Url { get; private set; }
    }
}