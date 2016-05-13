using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using URSA.Web.Converters;
using URSA.Web.Http.Converters;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Binds arguments from <see cref="FromQueryStringAttribute" />.</summary>
    public class FromBodyArgumentBinder : IParameterSourceArgumentBinder<FromBodyAttribute>
    {
        private readonly IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="FromBodyArgumentBinder" /> class.</summary>
        /// <param name="converterProvider">Converters provider</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        internal FromBodyArgumentBinder(IConverterProvider converterProvider)
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
            return GetArgumentValue(context as ArgumentBindingContext<FromBodyAttribute>);
        }

        /// <inheritdoc />
        public object GetArgumentValue(ArgumentBindingContext<FromBodyAttribute> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var contentType = context.Request.Headers[Header.ContentType];
            if (contentType != null)
            {
                if (contentType.Value.StartsWith("multipart/"))
                {
                    return GetArgumentValueFromMultipartBody(context);
                }

                IConverter xWwwUrlEncodedConverter;
                if ((contentType.Value == XWwwUrlEncodedConverter.ApplicationXWwwUrlEncoded) &&
                    (((xWwwUrlEncodedConverter = _converterProvider.FindBestInputConverter(context.Parameter.ParameterType, context.Request)) == null) ||
                    ((xWwwUrlEncodedConverter.CanConvertTo(context.Parameter.ParameterType, context.Request) & CompatibilityLevel.TypeMatch) == CompatibilityLevel.None)))
                {
                    return GetArgumentValueFromUrlEncodedString(context);
                }
            }

            var converter = _converterProvider.FindBestInputConverter(context.Parameter.ParameterType, context.Request);
            if (converter == null)
            {
                return null;
            }

            object result = converter.ConvertTo(context.Parameter.ParameterType, context.Request);
            context.Request.Body.Seek(0, SeekOrigin.Begin);
            return result;
        }

        private object GetArgumentValueFromUrlEncodedString(ArgumentBindingContext<FromBodyAttribute> context)
        {
            string data;
            using (var reader = new StreamReader(context.Request.Body))
            {
                data = reader.ReadToEnd();
            }

            context.Request.Body.Seek(0, SeekOrigin.Begin);
            if (data.Length <= 2)
            {
                return null;
            }

            data = "&" + data;
            string parameterName = GetParameterName(context);
            string template = String.Format("&{0}=(?<Value>[^&]+)", Regex.Escape(parameterName));
            MatchCollection matches = Regex.Matches(data, template);
            return (matches.Count == 0 ? null :
                _converterProvider.ConvertToCollection(
                    matches.Cast<Match>().Select(match => HttpUtility.UrlDecode(match.Groups["Value"].Value)),
                    context.Parameter.ParameterType,
                    context.Request));
        }

        private object GetArgumentValueFromMultipartBody(ArgumentBindingContext<FromBodyAttribute> context)
        {
            var contentType = context.Request.Headers[Header.ContentType].Values.First(value => value.Value.StartsWith("multipart/"));
            var boundary = contentType.Parameters[HeaderParameter.Boundary];
            if ((boundary == null) || (String.IsNullOrEmpty(boundary.Value as string)))
            {
                throw new InvalidOperationException("Cannot process multipart body without a boundary marker.");
            }

            if (!context.MultipartBodies.ContainsKey(context.Request))
            {
                context.MultipartBodies[context.Request] = ParseMultipartMessage(
                    context.Request.Method,
                    context.Request.Url,
                    new StreamReader(context.Request.Body).ReadToEnd(),
                    (string)boundary.Value);
                context.Request.Body.Seek(0, SeekOrigin.Begin);
            }

            if (contentType.Value == "multipart/form-data")
            {
                return GetArgumentValueFromMultipartFormDataBody(context);
            }

            return (!context.Success ? GetArgumentValueFromMultipartMixedBody(context) : null);
        }

        private object GetArgumentValueFromMultipartMixedBody(ArgumentBindingContext<FromBodyAttribute> context)
        {
            RequestInfo[] parts = context.MultipartBodies[context.Request];
            int partIndex = context.RequestMapping.Operation.UnderlyingMethod.GetParameters()
                .TakeWhile(argument => argument != context.Parameter)
                .Select(argument => argument.GetCustomAttribute<ParameterSourceAttribute>(true)).OfType<FromBodyAttribute>().Count();
            if (partIndex < parts.Length)
            {
                return GetArgumentValue(new ArgumentBindingContext<FromBodyAttribute>(
                    parts[partIndex],
                    context.RequestMapping,
                    context.Parameter,
                    context.Index,
                    context.ParameterSource,
                    context.MultipartBodies));
            }

            return null;
        }

        private object GetArgumentValueFromMultipartFormDataBody(ArgumentBindingContext<FromBodyAttribute> context)
        {
            RequestInfo[] parts = context.MultipartBodies[context.Request];
            string parameterName = GetParameterName(context);
            var part = (from item in parts
                        let contentDisposition = item.Headers[Header.ContentDisposition]
                        where contentDisposition != null
                        from value in contentDisposition.Values
                        where value.Value == "form-data"
                        from param in value.Parameters
                        where (param.Name == "name") && (param.Value is string) &&
                            (String.Compare((string)param.Value, parameterName, true) == 0)
                        select item).FirstOrDefault();
            if (part == null)
            {
                return null;
            }

            context.Success = true;
            return GetArgumentValue(new ArgumentBindingContext<FromBodyAttribute>(
                part,
                context.RequestMapping,
                context.Parameter,
                context.Index,
                context.ParameterSource,
                context.MultipartBodies));
        }

        private RequestInfo[] ParseMultipartMessage(Verb method, HttpUrl url, string content, string boundary)
        {
            content = content.Substring(boundary.Length + 4).Replace("--" + boundary + "--", String.Empty);
            string[] parts = Regex.Split(content, "--" + boundary + "\r\n");
            RequestInfo[] result = new RequestInfo[parts.Length];
            for (var index = 0; index < parts.Length; index++)
            {
                result[index] = RequestInfo.Parse(method, url, parts[index]);
            }

            return result;
        }

        private string GetParameterName(ArgumentBindingContext<FromBodyAttribute> context)
        {
            return (!String.IsNullOrEmpty(context.ParameterSource.Name) ?
                context.ParameterSource.Name :
                context.RequestMapping.Operation.UnderlyingMethod.GetParameters()
                    .Where(argument => argument == context.Parameter)
                    .Select(argument => argument.Name).First());
        }
    }
}