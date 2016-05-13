using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Binds arguments from <see cref="FromUrlAttribute" />.</summary>
    public class FromUrlArgumentBinder : IParameterSourceArgumentBinder<FromUrlAttribute>
    {
        private readonly IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="FromUrlArgumentBinder" /> class.</summary>
        /// <param name="converterProvider">Converters provider</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        internal FromUrlArgumentBinder(IConverterProvider converterProvider)
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
            return GetArgumentValue(context as ArgumentBindingContext<FromUrlAttribute>);
        }

        /// <inheritdoc />
        public object GetArgumentValue(ArgumentBindingContext<FromUrlAttribute> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (System.TypeExtensions.IsEnumerable(context.Parameter.ParameterType))
            {
                throw new InvalidOperationException(String.Format("Cannot bind types based on '{0}' to uri segments.", typeof(IEnumerable)));
            }

            string url = MakeUri(context.Parameter, context.RequestMapping.MethodRoute, context.RequestMapping.Operation);
            string template = UriTemplateBuilder.VariableTemplateRegex.Replace(url, "(?<Value>[^/\\?]+)");
            Match match = Regex.Match(context.Request.Url.AsRelative.ToString(), template);
            return (match.Success ? _converterProvider.ConvertTo(match.Groups["Value"].Value, context.Parameter.ParameterType, context.Request) : null);
        }

        internal static string MakeUri(ParameterInfo parameter, HttpUrl baseUrl, OperationInfo<Verb> operation)
        {
            string result = baseUrl.ToString();
            foreach (var argument in operation.Arguments)
            {
                var argumentSource = argument.Source;
                if (!(argumentSource is FromUrlAttribute))
                {
                    continue;
                }

                var url = ((FromUrlAttribute)argumentSource).UrlTemplate.Replace(FromUrlAttribute.Key, "/" + argument.Parameter.Name + "/").TrimStart('/');
                result += (result.EndsWith("/") ? String.Empty : "/") + url;
                if (argument.Parameter == parameter)
                {
                    break;
                }
            }

            return result;
        }
    }
}