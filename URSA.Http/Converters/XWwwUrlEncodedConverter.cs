using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using URSA.Web.Converters;
using URSA.Web.Http.Reflection;

namespace URSA.Web.Http.Converters
{
    /// <summary>Provides a conversion routines for <![CDATA[application/x-www-url-encoded]]> messages.</summary>
    public class XWwwUrlEncodedConverter : IConverter
    {
        internal const string ApplicationXWwwUrlEncoded = "application/x-www-url-encoded";

        private static readonly string[] MediaTypes = { ApplicationXWwwUrlEncoded };

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public IEnumerable<string> SupportedMediaTypes { get { return MediaTypes; } }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
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

            var requestInfo = request as RequestInfo;
            if (requestInfo == null)
            {
                throw new ArgumentOutOfRangeException("request");
            }

            var result = (requestInfo.Headers.ContentType.StartsWith(MediaTypes[0]) ? CompatibilityLevel.ExactProtocolMatch : CompatibilityLevel.None);
            TypeConverter typeConverter;
            if ((!expectedType.GetTypeInfo().IsValueType) && ((!(typeConverter = TypeDescriptor.GetConverter(expectedType)).CanConvertFrom(typeof(string))) || (typeConverter.GetType() == typeof(TypeConverter))))
            {
                result |= CompatibilityLevel.TypeMatch;
            }

            return result;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public T ConvertTo<T>(IRequestInfo request)
        {
            var result = ConvertTo(typeof(T), request);
            return (result == null ? default(T) : (T)result);
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

            var requestInfo = request as RequestInfo;
            if (requestInfo == null)
            {
                throw new ArgumentOutOfRangeException("request");
            }

            string data;
            using (var reader = new StreamReader(requestInfo.Body))
            {
                data = reader.ReadToEnd();
            }

            return ConvertTo(expectedType, data);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public T ConvertTo<T>(string body)
        {
            var result = ConvertTo(typeof(T), body);
            return (result == null ? default(T) : (T)result);
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

            var instance = Activator.CreateInstance(expectedType);
            StringBuilder propertyName = new StringBuilder(1024);
            StringBuilder value = new StringBuilder(1024);
            StringBuilder target = propertyName;
            foreach (char @char in body)
            {
                switch (@char)
                {
                    default:
                        target.Append(@char);
                        break;
                    case '=':
                        target = value;
                        break;
                    case '&':
                        SetPropertyValue(expectedType, propertyName.ToString(), value.ToString(), instance);
                        (target = propertyName).Clear();
                        value.Clear();
                        break;
                }
            }

            SetPropertyValue(expectedType, propertyName.ToString(), value.ToString(), instance);
            return instance;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
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

            var responseInfo = response as ResponseInfo;
            if (responseInfo == null)
            {
                throw new ArgumentOutOfRangeException("response");
            }

            var result = CompatibilityLevel.None;
            var accept = responseInfo.Request.Headers[Header.Accept];
            if ((accept != null) && (accept.Values.Join(SupportedMediaTypes, outer => outer.Value, inner => inner, (outer, inner) => inner).Any()))
            {
                result |= CompatibilityLevel.ExactProtocolMatch;
            }

            TypeConverter typeConverter;
            if ((!givenType.GetTypeInfo().IsValueType) && ((!(typeConverter = TypeDescriptor.GetConverter(givenType)).CanConvertTo(typeof(string))) || (typeConverter.GetType() == typeof(TypeConverter))))
            {
                result |= CompatibilityLevel.TypeMatch;
            }

            return result;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
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

            var responseInfo = response as ResponseInfo;
            if (responseInfo == null)
            {
                throw new ArgumentOutOfRangeException("response");
            }

            if (instance == null)
            {
                return;
            }

            if (!givenType.IsInstanceOfType(instance))
            {
                throw new InvalidOperationException();
            }

            using (var writer = new StreamWriter(responseInfo.Body))
            {
                string separator = String.Empty;
                foreach (var property in givenType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(property => (property.CanRead) && ((property.CanWrite) || (property.PropertyType.GetTypeInfo().IsEnumerable()))))
                {
                    var value = property.GetValue(instance);
                    if (value == null)
                    {
                        continue;
                    }

                    var typeConverter = TypeDescriptor.GetConverter(property.PropertyType.GetTypeInfo().GetItemType());
                    if ((value is IEnumerable) && (value.GetType() != typeof(string)))
                    {
                        if (value is byte[])
                        {
                            writer.Write("{0}{1}={2}", separator, property.Name, Convert.ToBase64String(responseInfo.Encoding.GetBytes((string)value)));
                        }
                        else
                        {
                            foreach (var item in (IEnumerable)value)
                            {
                                writer.Write("{0}{1}={2}", separator, property.Name, typeConverter.ConvertToInvariantString(item).UrlEncode());
                                separator = "&";
                            }
                        }
                    }
                    else
                    {
                        writer.Write("{0}{1}={2}", separator, property.Name, typeConverter.ConvertToInvariantString(value).UrlEncode());
                    }

                    separator = "&";
                }
            }
        }

        private void SetPropertyValue(Type expectedType, string propertyName, string value, object instance)
        {
            if (propertyName.Length <= 0)
            {
                return;
            }

            var matchingProperty = (from property in expectedType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    where (property.CanRead) && ((property.CanWrite) || (property.PropertyType.GetTypeInfo().IsEnumerable()))
                                        && (String.Compare(property.Name, propertyName, true) == 0)
                                    select property).FirstOrDefault();
            if (matchingProperty != null)
            {
                instance.SetPropertyValue(matchingProperty, value.UrlDecode());
            }
        }
    }
}