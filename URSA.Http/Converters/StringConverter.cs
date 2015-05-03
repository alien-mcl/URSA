using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using URSA.Web.Converters;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a <see cref="String" />.</summary>
    public class StringConverter : IConverter
    {
        /// <summary>Defines a 'text/plain' media type.</summary>
        public const string TextPlain = "text/plain";

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo<T>(IRequestInfo request)
        {
            return CanConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (expectedType.GetItemType() != typeof(string))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            RequestInfo requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            return (contentType != null) && (contentType.Values.Any(value => value == TextPlain)) ?
                result | CompatibilityLevel.ExactProtocolMatch :
                result;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(IRequestInfo request)
        {
            return (T)ConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string content = null;
            using (var reader = new StreamReader(request.Body))
            {
                content = reader.ReadToEnd();
            }

            return ConvertTo(expectedType, content);
        }

        /// <inheritdoc />
        public T ConvertTo<T>(string body)
        {
            return (T)ConvertTo(typeof(T), body);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, string body)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (body == null)
            {
                return (expectedType.IsValueType ? Activator.CreateInstance(expectedType) : null);
            }

            return (expectedType.IsEnumerable() ? Regex.Split(body, "\r\n").MakeInstance(expectedType, typeof(string)) : body);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom<T>(IResponseInfo response)
        {
            return CanConvertFrom(typeof(T), response);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response)
        {
            if (givenType == null)
            {
                throw new ArgumentNullException("givenType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (givenType.GetItemType() != typeof(string))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            var responseInfo = (ResponseInfo)response;
            var accept = responseInfo.Request.Headers[Header.Accept];
            if ((accept != null) && (accept.Values.Any(value => value == TextPlain)))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            return result;
        }

        /// <inheritdoc />
        public void ConvertFrom<T>(T instance, IResponseInfo response)
        {
            ConvertFrom(typeof(T), instance, response);
        }

        /// <inheritdoc />
        public void ConvertFrom(Type givenType, object instance, IResponseInfo response)
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
            responseInfo.Headers.ContentType = TextPlain;
            if (instance != null)
            {
                if (!givenType.IsInstanceOfType(instance))
                {
                    throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
                }

                string content = null;
                if (!givenType.IsEnumerable())
                {
                    content = (string)instance;
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
        }
    }
}