using System;
using System.Collections;
using System.ComponentModel;
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
    /// <summary>Binds arguments from <see cref="FromUriAttribute" />.</summary>
    public class FromUriArgumentBinder : IParameterSourceArgumentBinder<FromUriAttribute>
    {
        private readonly IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="FromUriArgumentBinder" /> class.</summary>
        /// <param name="converterProvider">Converters provider</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        internal FromUriArgumentBinder(IConverterProvider converterProvider)
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
            return GetArgumentValue(context as ArgumentBindingContext<FromUriAttribute>);
        }

        /// <inheritdoc />
        public object GetArgumentValue(ArgumentBindingContext<FromUriAttribute> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (System.TypeExtensions.IsEnumerable(context.Parameter.ParameterType))
            {
                throw new InvalidOperationException(String.Format("Cannot bind types based on '{0}' to uri segments.", typeof(IEnumerable)));
            }

            Uri uri = MakeUri(context.Parameter, context.RequestMapping.MethodRoute, context.RequestMapping.Operation);
            string template = UriTemplateBuilder.VariableTemplateRegex.Replace(uri.ToString(), "(?<Value>[^/\\?]+)");
            Match match = Regex.Match(context.Request.Uri.ToRelativeUri().ToString(), template);
            return (match.Success ? _converterProvider.ConvertTo(match.Groups["Value"].Value, context.Parameter.ParameterType, context.Request) : null);
        }

        private static Uri MakeUri(ParameterInfo parameter, Uri baseUri, OperationInfo<Verb> operation)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            Uri result = baseUri;
            foreach (var argument in operation.Arguments)
            {
                var argumentSource = argument.Source;
                if (!(argumentSource is FromUriAttribute))
                {
                    continue;
                }

                var uri = ((FromUriAttribute)argumentSource).UriTemplate.ToString().Replace(FromUriAttribute.Key, "/" + argument.Parameter.Name + "/");
                result = new Uri(uri, UriKind.RelativeOrAbsolute).Combine(result);
                if (argument.Parameter == parameter)
                {
                    break;
                }
            }

            return result;
        }
    }
}