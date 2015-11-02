using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

            if (context.Request.Uri.Query.Length < 2)
            {
                return null;
            }

            string template;
            var variableMatch = UriTemplate.VariableTemplateRegex.Match(context.ParameterSource.UriTemplate);
            template = variableMatch.Success ? 
                String.Format("{0}=(?<Value>[^&]+)", (variableMatch.Groups["ExpansionType"].Success ? variableMatch.Groups["ParameterName"].Value : context.Parameter.Name)) :
                context.ParameterSource.UriTemplate.Replace(FromQueryStringAttribute.Key, "&" + context.Parameter.Name + "=");

            MatchCollection matches = Regex.Matches(context.Request.Uri.Query.Substring(1), template);
            return (matches.Count == 0 ? null : 
                _converterProvider.ConvertToCollection(matches.Cast<Match>().Select(match => match.Groups["Value"].Value), context.Parameter.ParameterType, context.Request));
        }
    }
}