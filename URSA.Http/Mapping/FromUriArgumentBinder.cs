using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Web.Converters;
using URSA.Web.Description;
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

            if (context.Parameter.ParameterType.IsEnumerable())
            {
                throw new InvalidOperationException(String.Format("Cannot bind types based on '{0}' to uri segments.", typeof(IEnumerable)));
            }

            Uri uri = MakeUri(context.Parameter, context.RequestMapping.MethodRoute, context.RequestMapping.Operation);
            string template = uri.ToString()
                .Replace("/" + context.Parameter.Name + "/" + FromUriAttribute.Value, "/" + context.Parameter.Name + "/(?<Value>[^/\\?]+)")
                .Replace(FromUriAttribute.Value, "[^/\\?]+");
            Match match = Regex.Match(context.Request.Uri.ToString(), template);
            if (!match.Success)
            {
                return null;
            }

            bool success;
            object result = ConvertUsingTypeConverters(context, match, out success);
            if (!success)
            {
                result = ConvertUsingCustomConverters(context, match);
            }

            return result;
        }

        private object ConvertUsingTypeConverters(ArgumentBindingContext<FromUriAttribute> context, Match match, out bool success)
        {
            success = false;
            var converter = TypeDescriptor.GetConverter(context.Parameter.ParameterType);
            if ((converter != null) && (converter.CanConvertFrom(typeof(string))))
            {
                success = true;
                return converter.ConvertFromInvariantString(match.Groups["Value"].Value);
            }

            return null;
        }

        private object ConvertUsingCustomConverters(ArgumentBindingContext<FromUriAttribute> context, Match match)
        {
            var converter = _converterProvider.FindBestInputConverter(context.Parameter.ParameterType, context.Request, true);
            if (converter != null)
            {
                return converter.ConvertTo(context.Parameter.ParameterType, match.Groups["Value"].Value);
            }

            return null;
        }

        private Uri MakeUri(ParameterInfo parameter, Uri baseUri, OperationInfo<Verb> operation)
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
                if ((argumentSource == null) || (!(argumentSource is FromUriAttribute)))
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