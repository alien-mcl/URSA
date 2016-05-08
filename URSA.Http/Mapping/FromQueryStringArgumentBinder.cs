using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using URSA.Web.Converters;
using URSA.Web.Description.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Binds arguments from <see cref="FromQueryStringAttribute" />.</summary>
    public class FromQueryStringArgumentBinder : IParameterSourceArgumentBinder<FromQueryStringAttribute>
    {
        private readonly IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="FromQueryStringArgumentBinder" /> class.</summary>
        /// <param name="converterProvider">Converters provider</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        internal FromQueryStringArgumentBinder(IConverterProvider converterProvider)
        {
            if (converterProvider == null)
            {
                throw new ArgumentNullException("converterProvider");
            }

            _converterProvider = converterProvider;
        }

        /// <inheritdoc />
        public object GetArgumentValue(ArgumentBindingContext context)
        {
            return GetArgumentValue(context as ArgumentBindingContext<FromQueryStringAttribute>);
        }

        /// <inheritdoc />
        public object GetArgumentValue(ArgumentBindingContext<FromQueryStringAttribute> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if ((!context.Request.Url.HasQuery) || (context.Request.Url.Query.Count == 0))
            {
                return null;
            }

            string parameterName = context.Parameter.Name;
            var variableMatch = UriTemplateBuilder.VariableTemplateRegex.Match(context.ParameterSource.UrlTemplate);
            if ((!variableMatch.Success) || (!variableMatch.Groups["ExpansionType"].Success))
            {
                variableMatch = Regex.Match(context.ParameterSource.UrlTemplate, "[?&]*(<ParameterName>[^=]+)=");
            }

            parameterName = (variableMatch.Success ? variableMatch.Groups["ParameterName"].Value : parameterName);
            var values = context.Request.Url.Query.GetValues(parameterName);
            return (!values.Any() ? null : _converterProvider.ConvertToCollection(values, context.Parameter.ParameterType, context.Request));
        }
    }
}