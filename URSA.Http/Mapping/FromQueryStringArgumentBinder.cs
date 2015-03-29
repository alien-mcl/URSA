using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Web.Converters;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Binds arguments from <see cref="FromQueryStringAttribute" />.</summary>
    public class FromQueryStringArgumentBinder : IParameterSourceArgumentBinder<FromQueryStringAttribute>
    {
        private IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="FromQueryStringArgumentBinder" /> class.</summary>
        /// <param name="converterProvider">Converters provider</param>
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

            string template = context.ParameterSource.UriTemplate
                .Replace(FromQueryStringAttribute.Value, "(?<Value>[^&]+)")
                .Replace(FromQueryStringAttribute.Key, "&" + context.Parameter.Name + "=");
            MatchCollection matches = Regex.Matches("&" + context.Request.Uri.Query.Substring(1), template);
            if (matches.Count == 0)
            {
                return null;
            }

            Type itemType = context.Parameter.ParameterType.GetItemType();
            bool success;
            object result = ConvertUsingTypeConverters(context, matches, itemType, out success);
            if (!success)
            {
                result = ConvertUsingCustomConverters(context, matches, itemType);
            }

            return result;
        }

        private object ConvertUsingTypeConverters(ArgumentBindingContext<FromQueryStringAttribute> context, MatchCollection matches, Type itemType, out bool success)
        {
            success = false;
            var converter = TypeDescriptor.GetConverter(itemType);
            if ((converter != null) && (converter.CanConvertFrom(typeof(string))))
            {
                success = true;
                var values = matches.Cast<Match>().Select(match => converter.ConvertFromInvariantString(match.Groups["Value"].Value));
                return values.MakeInstance(context.Parameter.ParameterType, itemType);
            }

            return null;
        }

        private object ConvertUsingCustomConverters(ArgumentBindingContext<FromQueryStringAttribute> context, MatchCollection matches, Type itemType)
        {
            var converter = _converterProvider.FindBestInputConverter(context.Parameter.ParameterType, context.Request, true);
            if (converter != null)
            {
                var values = matches.Cast<Match>().Select(match => converter.ConvertTo(itemType, match.Groups["Value"].Value));
                return values.MakeInstance(context.Parameter.ParameterType, itemType);
            }

            return null;
        }
    }
}