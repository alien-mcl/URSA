using System;
using System.Collections.Generic;
using URSA.Web.Converters;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts exceptions into an HTTP message.</summary>
    public class ExceptionConverter : IConverter
    {
        private static readonly string[] MediaTypes = new string[0];

        /// <inheritdoc />
        public IEnumerable<string> SupportedMediaTypes { get { return MediaTypes; } }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo<T>(IRequestInfo request)
        {
            return CompatibilityLevel.None;
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request)
        {
            return CompatibilityLevel.None;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(IRequestInfo request)
        {
            return default(T);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, IRequestInfo request)
        {
            return null;
        }

        /// <inheritdoc />
        public T ConvertTo<T>(string body)
        {
            return default(T);
        }

        /// <inheritdoc />
        public object ConvertTo(Type expectedType, string body)
        {
            return null;
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom<T>(IResponseInfo response)
        {
            return CanConvertFrom(typeof(T), response);
        }

        /// <inheritdoc />
        public CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var result = CompatibilityLevel.ProtocolMatch;
            if (response is ExceptionResponseInfo)
            {
                result |= CompatibilityLevel.ExactTypeMatch;
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

            if (instance == null)
            {
                return;
            }

            if (!givenType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
            }

            ProtocolException exception = ((Exception)instance).AsHttpException();
            ResponseInfo responseInfo = (ResponseInfo)response;
            var message = String.Format(
                "{0:000} {1} \"{2}\"",
                (uint)exception.HResult % 999,
                exception.Source ?? responseInfo.Request.Uri.Host,
                System.Web.HttpUtility.JavaScriptStringEncode(exception.Message));
            responseInfo.Headers.Add(new Header(Header.Warning, message));
            responseInfo.Status = exception.Status;
        }
    }
}