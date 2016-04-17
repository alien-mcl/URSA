using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using URSA.Web.Converters;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a <see cref="Uri" />.</summary>
    public class UriConverter : SpecializedLiteralConverter<Uri>
    {
        /// <summary>Defines a 'text/uri-list' media type.</summary>
        public const string TextUriList = "text/uri-list";

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        protected override int MaxBodyLength { get { return 4096; } }

        /// <inheritdoc />
        public override CompatibilityLevel CanConvertTo<T>(IRequestInfo request)
        {
            return CanConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public override CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (expectedType.GetItemType() != typeof(Uri))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            RequestInfo requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            if ((contentType != null) && (contentType.Values.Any(value => value == TextUriList)))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }
            else
            {
                result = base.CanConvertTo(expectedType, request);
            }

            return result;
        }

        /// <inheritdoc />
        public override T ConvertTo<T>(IRequestInfo request)
        {
            return (T)ConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public override object ConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            if ((contentType != null) && (contentType.Values.Any(value => value.Value == TextUriList)))
            {
                string content = null;
                using (var reader = new StreamReader(request.Body))
                {
                    content = reader.ReadToEnd();
                }

                return ConvertTo(expectedType, content);
            }
            else
            {
                return base.ConvertTo(expectedType, request);
            }
        }

        /// <inheritdoc />
        public override CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response)
        {
            if (givenType == null)
            {
                throw new ArgumentNullException("givenType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (givenType.GetItemType() != typeof(Uri))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            var responseInfo = (ResponseInfo)response;
            var accept = responseInfo.Request.Headers[Header.Accept];
            if ((accept != null) && (accept.Values.Any(value => value == TextUriList)))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }
            else
            {
                result = base.CanConvertFrom(givenType, response);
            }

            return result;
        }

        /// <inheritdoc />
        public override CompatibilityLevel CanConvertFrom<T>(IResponseInfo response)
        {
            return CanConvertFrom(typeof(T), response);
        }

        /// <inheritdoc />
        public override void ConvertFrom<T>(T instance, IResponseInfo response)
        {
            ConvertFrom(typeof(T), instance, response);
        }

        /// <inheritdoc />
        public override void ConvertFrom(Type givenType, object instance, IResponseInfo response)
        {
            if (givenType == null)
            {
                throw new ArgumentNullException("givenType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var responseInfo = (ResponseInfo)response;
            var contentType = responseInfo.Request.Headers[Header.ContentType];
            if ((contentType != null) && (contentType.Values.Any(value => value.Value == TextUriList)))
            {
                responseInfo.Headers.ContentType = TextUriList;
                if (instance != null)
                {
                    if (!givenType.IsInstanceOfType(instance))
                    {
                        throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
                    }

                    string content = null;
                    if (!System.TypeExtensions.IsEnumerable(givenType))
                    {
                        content = instance.ToString();
                    }
                    else
                    {
                        var builder = new StringBuilder(1024);
                        foreach (var item in (IEnumerable)instance)
                        {
                            builder.AppendFormat("{0}\r\n", item);
                        }

                        content = builder.ToString().TrimEnd('\r', '\n');
                    }

                    using (var writer = new StreamWriter(responseInfo.Body))
                    {
                        writer.Write(content);
                        writer.Flush();
                    }
                }
                else if (responseInfo.Status == HttpStatusCode.OK)
                {
                    responseInfo.Status = HttpStatusCode.NoContent;
                }
            }
            else
            {
                base.ConvertFrom(givenType, instance, response);
            }
        }

        /// <inheritdoc />
        protected override bool CanConvert(Type expectedType)
        {
            return expectedType.GetItemType() == typeof(Uri);
        }
    }
}