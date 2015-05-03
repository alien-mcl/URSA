using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using URSA.Web.Converters;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a strongly typed literal.</summary>
    /// <typeparam name="TT">Type of the objects this converter can handle.</typeparam>
    public abstract class SpecializedLiteralConverter<TT> : IConverter
    {
        /// <summary>Gets the minimal body length required for the conversion to be possible.</summary>
        /// <remarks>Any negative value or 0 is considered as no limits.</remarks>
        protected abstract int MaxBodyLength { get; }

        /// <inheritdoc />
        public virtual CompatibilityLevel CanConvertTo<T>(IRequestInfo request)
        {
            return CanConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public virtual CompatibilityLevel CanConvertTo(Type expectedType, IRequestInfo request)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (!CanConvert(expectedType))
            {
                return CompatibilityLevel.None;
            }

            var requestInfo = (RequestInfo)request;
            if ((!expectedType.IsEnumerable()) && (requestInfo.Headers.ContentLength > 0) && (requestInfo.Headers.ContentLength > MaxBodyLength))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            var contentType = requestInfo.Headers[Header.ContentType];
            if ((contentType == null) || (!contentType.Values.Any(value => value == StringConverter.TextPlain)))
            {
                return result;
            }

            var itemType = expectedType.GetItemType();
            using (var reader = new StreamReader(request.Body))
            {
                string content = reader.ReadToEnd();
                if (String.IsNullOrEmpty(content))
                {
                    return result;
                }

                object value = ParseValue(itemType, content);
                request.Body.Seek(0, SeekOrigin.Begin);
                if (value != null)
                {
                    result |= CompatibilityLevel.ProtocolMatch;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public virtual T ConvertTo<T>(IRequestInfo request)
        {
            return (T)ConvertTo(typeof(T), request);
        }

        /// <inheritdoc />
        public virtual object ConvertTo(Type expectedType, IRequestInfo request)
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
        public virtual T ConvertTo<T>(string body)
        {
            return (T)ConvertTo(typeof(T), body);
        }

        /// <inheritdoc />
        public virtual object ConvertTo(Type expectedType, string body)
        {
            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (String.IsNullOrEmpty(body))
            {
                return (expectedType.IsValueType ? Activator.CreateInstance(expectedType) : null);
            }

            bool isEnumerable = expectedType.IsEnumerable();
            Type itemType = expectedType.GetItemType();
            IList<object> result = new List<object>();
            foreach (var item in Regex.Split(body, "\r\n"))
            {
                object value = ParseValue(itemType, item);
                if (value != null)
                {
                    value = System.Convert.ChangeType(value, itemType);
                    if (!isEnumerable)
                    {
                        return value;
                    }

                    result.Add(value);
                }
            }

            if ((isEnumerable) && (result.Count == 0))
            {
                return (expectedType.IsValueType ? Activator.CreateInstance(expectedType) : null);
            }

            return result.MakeInstance(expectedType, expectedType.GetItemType());
        }

        /// <inheritdoc />
        public virtual CompatibilityLevel CanConvertFrom<T>(IResponseInfo response)
        {
            return CanConvertFrom(typeof(T), response);
        }

        /// <inheritdoc />
        public virtual CompatibilityLevel CanConvertFrom(Type givenType, IResponseInfo response)
        {
            if (givenType == null)
            {
                throw new ArgumentNullException("givenType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (!CanConvert(givenType))
            {
                return CompatibilityLevel.None;
            }

            var result = CompatibilityLevel.ExactTypeMatch;
            var responseInfo = (ResponseInfo)response;
            var accept = responseInfo.Request.Headers[Header.Accept];
            if ((accept != null) && (accept.Values.Any(value => value == StringConverter.TextPlain)))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            return result;
        }

        /// <inheritdoc />
        public virtual void ConvertFrom<T>(T instance, IResponseInfo response)
        {
            ConvertFrom(typeof(T), instance, response);
        }

        /// <inheritdoc />
        public virtual void ConvertFrom(Type givenType, object instance, IResponseInfo response)
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
            responseInfo.Headers.ContentType = StringConverter.TextPlain;
            if (instance == null)
            {
                return;
            }

            if (!givenType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException(String.Format("Instance type '{0}' mismatch from the given '{1}'.", instance.GetType(), givenType));
            }

            string content = null;
            if (!givenType.IsEnumerable())
            {
                content = String.Format(CultureInfo.InvariantCulture, "{0}", instance);
            }
            else
            {
                var builder = new StringBuilder(1024);
                foreach (var item in (IEnumerable)instance)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n", item);
                }

                content = builder.ToString().TrimEnd('\r', '\n');
            }

            using (var writer = new StreamWriter(responseInfo.Body))
            {
                writer.Write(content);
                writer.Flush();
            }
        }

        /// <summary>Checks if it is possible to convert to given type.</summary>
        /// <param name="expectedType">Expected type.</param>
        /// <returns><b>true</b> if the converter actually can return given <typeparamref name="T" />; otherwise <b>false</b>.</returns>
        protected abstract bool CanConvert(Type expectedType);

        /// <summary>Parses given value into an literal.</summary>
        /// <param name="expectedType">Expected item type.</param>
        /// <param name="value">Value to be parsed.</param>
        /// <returns>Parsed object.</returns>
        protected virtual object ParseValue(Type expectedType, string value)
        {
            return TypeDescriptor.GetConverter(expectedType).ConvertFromInvariantString(value);
        }
    }
}