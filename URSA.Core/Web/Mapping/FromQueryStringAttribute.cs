using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace URSA.Web.Mapping
{
    /// <summary>Marks the parameter to be bound to the query string.</summary>
    public sealed class FromQueryStringAttribute : ParameterSourceAttribute, IUrlTemplateParameterSourceAttribute
    {
        /// <summary>Defines a part of the URL responsible for mapping the actual value of the parameter in the URL.</summary>
        public const string Value = "{value}";

        /// <summary>Defines a part of the URL responsible for mapping name of the parameter.</summary>
        public const string Key = "&key=";

        /// <summary>Defines a default query string pattern.</summary>
        public static readonly string Default = "{?key}";

        private string _default;

        /// <summary>Initializes a new instance of the <see cref="FromQueryStringAttribute" /> class.</summary>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public FromQueryStringAttribute()
        {
            UrlTemplate = Default;
        }

        /// <summary>Initializes a new instance of the <see cref="FromQueryStringAttribute" /> class.</summary>
        /// <param name="url">Template of the URL for this parameter mapping.</param>
        public FromQueryStringAttribute(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            if (url.Length == 0)
            {
                throw new ArgumentOutOfRangeException("url");
            }

            if ((UrlTemplate = url.Trim('&', '?')).IndexOf('{') == -1)
            {
                throw new ArgumentOutOfRangeException("url");
            }
        }

        /// <summary>Gets the template of the URL for this parameter mapping.</summary>
        public string UrlTemplate { get; private set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        string IUrlTemplateParameterSourceAttribute.Template { get { return UrlTemplate; } }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        string IUrlTemplateParameterSourceAttribute.DefaultTemplate { get { return _default ?? Default; } }

        /// <summary>Creates an instance of the <see cref="FromQueryStringAttribute" /> for given <paramref name="parameter "/>.</summary>
        /// <param name="parameter">Parameter for which the attribute needs to be created.</param>
        /// <returns>Instance of the <see cref="FromQueryStringAttribute" />.</returns>
        public static FromQueryStringAttribute For(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            var format = ((!parameter.HasDefaultValue) && (parameter.ParameterType.IsValueType) ? "&{0}={{{0}}}" :
                "{{?{0}" + (System.Reflection.TypeExtensions.IsEnumerable(parameter.ParameterType) ? "*}}" : "}}"));
            var result = new FromQueryStringAttribute(String.Format(format, parameter.Name));
            result._default = ((!parameter.HasDefaultValue) && (parameter.ParameterType.IsValueType) ? "&key={value}" :
                "{?key" + (System.Reflection.TypeExtensions.IsEnumerable(parameter.ParameterType) ? "*}" : "}"));
            return result;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        ParameterSourceAttribute IUrlTemplateParameterSourceAttribute.For(ParameterInfo parameter)
        {
            return For(parameter);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public override string ToString()
        {
            return UrlTemplate;
        }
    }
}
