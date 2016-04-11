using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace URSA.Web.Mapping
{
    /// <summary>Marks the parameter to be bound to the uri.</summary>
    public sealed class FromUriAttribute : ParameterSourceAttribute, IUriTemplateParameterSourceAttribute
    {
        /// <summary>Defines a part of the uri responsible for mapping the actual value of the parameter in the uri.</summary>
        public const string Value = "{value}";

        /// <summary>Defines a part of the uri responsible for mapping the name of the parameter.</summary>
        public const string Key = "/key/";

        /// <summary>Defines a default uri pattern.</summary>
        public static readonly Uri Default = new Uri("/{value}", UriKind.Relative);

        /// <summary>Initializes a new instance of the <see cref="FromUriAttribute" /> class.</summary>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public FromUriAttribute()
        {
            UriTemplate = Default;
        }

        /// <summary>Initializes a new instance of the <see cref="FromUriAttribute" /> class.</summary>
        /// <param name="uri">Template of the uri for this parameter mapping.</param>
        public FromUriAttribute(string uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Length == 0)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            if ((UriTemplate = new Uri(uri, UriKind.Relative)).ToString().IndexOf('{') == -1)
            {
                throw new ArgumentOutOfRangeException("uri");
            }
        }

        /// <summary>Gets the template of the uri for this parameter mapping.</summary>
        public Uri UriTemplate { get; private set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        string IUriTemplateParameterSourceAttribute.Template { get { return UriTemplate.ToString(); } }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        string IUriTemplateParameterSourceAttribute.DefaultTemplate { get { return Default.ToString(); } }

        /// <summary>Creates an instance of the <see cref="FromUriAttribute" /> for given <paramref name="parameter "/>.</summary>
        /// <param name="parameter">Parameter for which the attribute needs to be created.</param>
        /// <returns>Instance of the <see cref="FromQueryStringAttribute" />.</returns>
        public static FromUriAttribute For(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            return new FromUriAttribute(String.Format("/{{{0}}}", parameter.Name));
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        ParameterSourceAttribute IUriTemplateParameterSourceAttribute.For(ParameterInfo parameter)
        {
            return For(parameter);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public override string ToString()
        {
            return UriTemplate.ToString();
        }
    }
}
