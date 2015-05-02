using System;
using System.IO;
using System.Linq;
using URSA.Web.Converters;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into an array of <see cref="Byte" />s.</summary>
    public class BinaryConverter : IConverter
    {
        /// <summary>Defines an '*/*' media type.</summary>
        public const string AnyAny = "*/*";

        /// <summary>Defines a 'application/octet-stream' media type.</summary>
        public const string ApplicationOctetStream = "application/octet-stream";

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

            if (expectedType != typeof(byte[]))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            RequestInfo requestInfo = (RequestInfo)request;
            var contentType = requestInfo.Headers[Header.ContentType];
            return (contentType != null) && (contentType.Values.Any(value => value == ApplicationOctetStream)) ?
                result | CompatibilityLevel.ExactProtocolMatch :
                result | CompatibilityLevel.ProtocolMatch;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(IRequestInfo request)
        {
            return (T)ConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, IRequestInfo request)
        {
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
                return null;
            }

            return System.Convert.FromBase64String(body);
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

            if (givenType != typeof(byte[]))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            var responseInfo = (ResponseInfo)response;
            var accept = responseInfo.Request.Headers[Header.Accept];
            if (accept != null)
            {
                if (accept.Values.Any(value => (value.Value == ApplicationOctetStream)))
                {
                    result |= CompatibilityLevel.ExactProtocolMatch;
                }
                else if (accept.Values.Any(value => (value.Value == AnyAny)))
                {
                    result |= CompatibilityLevel.ProtocolMatch;
                }
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
            var responseInfo = (ResponseInfo)response;
            responseInfo.Headers.ContentType = ApplicationOctetStream;
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (instance != null)
            {
                if (!givenType.IsAssignableFrom(instance.GetType()))
                {
                    throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
                }

                using (var writer = new StreamWriter(responseInfo.Body))
                {
                    writer.Write(Convert.ToBase64String((byte[])instance));
                    writer.Flush();
                }
            }
        }
    }
}