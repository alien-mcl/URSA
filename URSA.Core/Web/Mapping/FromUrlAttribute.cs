using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using URSA.Web.Http;

namespace URSA.Web.Mapping
{
    /// <summary>Marks the parameter to be bound to the URL.</summary>
    public sealed class FromUrlAttribute : ParameterSourceAttribute, IUrlTemplateParameterSourceAttribute
    {
        /// <summary>Defines a part of the URL responsible for mapping the actual value of the parameter in the URL.</summary>
        public const string Value = "{value}";

        /// <summary>Defines a part of the URL responsible for mapping the name of the parameter.</summary>
        public const string Key = "/key/";

        /// <summary>Defines a default URL pattern.</summary>
        public static readonly string Default = "/{value}";

        /// <summary>Initializes a new instance of the <see cref="FromUrlAttribute" /> class.</summary>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public FromUrlAttribute()
        {
            UrlTemplate = Default;
        }

        /// <summary>Initializes a new instance of the <see cref="FromUrlAttribute" /> class.</summary>
        /// <param name="url">Template of the URL for this parameter mapping.</param>
        public FromUrlAttribute(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            if (url.Length == 0)
            {
                throw new ArgumentOutOfRangeException("url");
            }

            if ((UrlTemplate = url).IndexOf('{') == -1)
            {
                throw new ArgumentOutOfRangeException("url");
            }
        }

        /// <summary>Gets the template of the URL for this parameter mapping.</summary>
        public string UrlTemplate { get; private set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        string IUrlTemplateParameterSourceAttribute.Template { get { return UrlTemplate.ToString(); } }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        string IUrlTemplateParameterSourceAttribute.DefaultTemplate { get { return Default.ToString(); } }

        /// <summary>Creates an instance of the <see cref="FromUrlAttribute" /> for given <paramref name="parameter "/>.</summary>
        /// <param name="parameter">Parameter for which the attribute needs to be created.</param>
        /// <returns>Instance of the <see cref="FromQueryStringAttribute" />.</returns>
        public static FromUrlAttribute For(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            return new FromUrlAttribute(String.Format("/{{{0}}}", parameter.Name));
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
            return UrlTemplate.ToString();
        }
    }
}
