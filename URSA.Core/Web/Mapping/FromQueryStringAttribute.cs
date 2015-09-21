using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace URSA.Web.Mapping
{
    /// <summary>Marks the parameter to be bound to the query string.</summary>
    [ExcludeFromCodeCoverage]
    public sealed class FromQueryStringAttribute : ParameterSourceAttribute
    {
        /// <summary>Defines a part of the uri responsible for mapping the actual value of the parameter in the uri.</summary>
        public const string Value = "{?value}";

        /// <summary>Defines a part of the uri responsible for mapping name of the parameter.</summary>
        public const string Key = "&key=";

        /// <summary>Defines a default query string pattern.</summary>
        public static readonly string Default = String.Format("{0}{1}", Key, Value);

        /// <summary>Initializes a new instance of the <see cref="FromQueryStringAttribute" /> class.</summary>
        public FromQueryStringAttribute()
        {
            UriTemplate = Default;
        }

        /// <summary>Initializes a new instance of the <see cref="FromQueryStringAttribute" /> class.</summary>
        /// <param name="uri">Template of the uri for this parameter mapping.</param>
        public FromQueryStringAttribute(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Length == 0)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            UriTemplate = (uri[0] == '&' ? String.Empty : "&") + (uri[0] == '?' ? uri.Substring(1) : uri);
        }

        /// <summary>Gets the template of the uri for this parameter mapping.</summary>
        public string UriTemplate { get; private set; }

        /// <summary>Creates an instance of the <see cref="FromQueryStringAttribute" /> for given <paramref name="parameter "/>.</summary>
        /// <param name="parameter">Parameter for which the attribute needs to be created.</param>
        /// <returns>Instance of the <see cref="FromQueryStringAttribute" />.</returns>
        public static FromQueryStringAttribute For(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            return new FromQueryStringAttribute(String.Format("&{0}={1}", parameter.Name, Value));
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return UriTemplate;
        }
    }
}
