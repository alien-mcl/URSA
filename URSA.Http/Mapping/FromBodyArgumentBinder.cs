using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using URSA.Web.Converters;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Binds arguments from <see cref="FromQueryStringAttribute" />.</summary>
    public class FromBodyArgumentBinder : IParameterSourceArgumentBinder<FromBodyAttribute>
    {
        private readonly IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="FromQueryStringArgumentBinder" /> class.</summary>
        /// <param name="converterProvider">Converters provider</param>
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
            if ((contentType != null) && (contentType.Value.StartsWith("multipart/")))
            {
                return GetArgumentValueFromMultipartBody(context);
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

        private object GetArgumentValueFromMultipartBody(ArgumentBindingContext<FromBodyAttribute> context)
        {
            var contentType = context.Request.Headers[Header.ContentType].Values.First(value => value.Value.StartsWith("multipart/"));
            var boundary = contentType.Parameters[HeaderParameter.Boundary];
            if ((boundary == null) || (!(boundary.Value is string)) || (String.IsNullOrEmpty((string)boundary.Value)))
            {
                throw new InvalidOperationException("Cannot process multipart body without a boundary marker.");
            }

            if (!context.MultipartBodies.ContainsKey(context.Request))
            {
                context.MultipartBodies[context.Request] = ParseMultipartMessage(
                    context.Request.Method,
                    context.Request.Uri,
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
            var part = (from item in parts
                        let contentDisposition = item.Headers[Header.ContentDisposition]
                        where contentDisposition != null
                        from value in contentDisposition.Values
                        where value.Value == "form-data"
                        from param in value.Parameters
                        where (param.Name == "name") && (param.Value is string) && 
                            (String.Compare((string)param.Value, context.ParameterSource.Name, true) == 0)
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

        private RequestInfo[] ParseMultipartMessage(Verb method, Uri uri, string content, string boundary)
        {
            content = content.Substring(boundary.Length + 4).Replace("--" + boundary + "--", String.Empty);
            string[] parts = Regex.Split(content, "--" + boundary + "\r\n");
            RequestInfo[] result = new RequestInfo[parts.Length];
            for (var index = 0; index < parts.Length; index++)
            {
                result[index] = RequestInfo.Parse(method, uri, parts[index]);
            }

            return result;
        }
    }
}