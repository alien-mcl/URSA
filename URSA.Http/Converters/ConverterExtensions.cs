using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using URSA.Web.Converters;

namespace URSA.Web.Http.Converters
{
    /// <summary>Provides useful <see cref="IConverter" /> helpers.</summary>
    public static class ConverterExtensions
    {
        /// <summary>Converts string to given type.</summary>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The target type.</param>
        /// <param name="request">The request.</param>
        /// <returns>Instance of the <paramref name="type" />.</returns>
        public static object ConvertTo(this IConverterProvider converterProvider, string value, Type type, IRequestInfo request = null)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (value == null)
            {
                return (type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null);
            }

            bool success;
            object result = ConvertUsingTypeConverters(value, type, out success);
            if ((success) || (request == null))
            {
                return result;
            }

            if (converterProvider == null)
            {
                throw new ArgumentNullException("converterProvider");
            }

            return ConvertUsingCustomConverters(converterProvider, request, value, type);
        }

        /// <summary>Converts an object into a string response body.</summary>
        /// <param name="converterProvider">Converter provider.</param>
        /// <param name="value">The value.</param>
        /// <param name="response">Optional response.</param>
        /// <returns>String representation of the given <paramref name="value" />.</returns>
        public static string ConvertFrom(this IConverterProvider converterProvider, object value, IResponseInfo response = null)
        {
            if (value == null)
            {
                return null;
            }

            bool success;
            string result = ConvertUsingTypeConverters(value, out success);
            if (success)
            {
                return result;
            }

            if (converterProvider == null)
            {
                throw new ArgumentNullException("converterProvider");
            }

            return ConvertUsingCustomConverters(converterProvider, response, value);
        }

        /// <summary>Converts string values to given collection.</summary>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="values">The values.</param>
        /// <param name="collectionType">The collection type.</param>
        /// <param name="request">The request.</param>
        /// <returns>Instance of the <paramref name="collectionType" />.</returns>
        public static object ConvertToCollection(this IConverterProvider converterProvider, IEnumerable<string> values, Type collectionType, IRequestInfo request = null)
        {
            if (values == null)
            {
                return null;
            }

            if (collectionType == null)
            {
                throw new ArgumentNullException("collectionType");
            }

            var collectionTypeInfo = collectionType.GetTypeInfo();
            var itemType = collectionTypeInfo.GetItemType();
            bool success;
            var result = ConvertUsingTypeConverters(values, itemType, out success);
            if ((success) || (request == null))
            {
                return result.MakeInstance(collectionTypeInfo, itemType);
            }

            if (converterProvider == null)
            {
                throw new ArgumentNullException("converterProvider");
            }

            return ConvertUsingCustomConverters(converterProvider, request, values, itemType).MakeInstance(collectionTypeInfo, itemType);
        }

        private static IEnumerable<object> ConvertUsingTypeConverters(IEnumerable<string> values, Type itemType, out bool success)
        {
            success = false;
            var converter = TypeDescriptor.GetConverter(itemType);
            if ((converter == null) || (!converter.CanConvertFrom(typeof(string))))
            {
                return null;
            }

            success = true;
            return values.Select(value => converter.ConvertFromInvariantString(value));
        }

        private static IEnumerable<object> ConvertUsingCustomConverters(IConverterProvider converterProvider, IRequestInfo request, IEnumerable<string> values, Type itemType)
        {
            var converter = converterProvider.FindBestInputConverter(itemType, request, true);
            return (converter == null ? null : values.Select(value => converter.ConvertTo(itemType, value)));
        }

        private static object ConvertUsingTypeConverters(string value, Type type, out bool success)
        {
            success = false;
            var converter = TypeDescriptor.GetConverter(type);
            if ((converter == null) || (!converter.CanConvertFrom(typeof(string))))
            {
                return null;
            }

            success = true;
            return converter.ConvertFromInvariantString(value);
        }

        private static object ConvertUsingCustomConverters(IConverterProvider converterProvider, IRequestInfo request, string value, Type type)
        {
            var converter = converterProvider.FindBestInputConverter(type, request, true);
            return (converter == null ? null : converter.ConvertTo(type, value));
        }

        private static string ConvertUsingTypeConverters(object value, out bool success)
        {
            success = false;
            var converter = TypeDescriptor.GetConverter(value.GetType());
            if ((converter == null) || (!converter.CanConvertTo(typeof(string))) || (converter.GetType() == typeof(TypeConverter)))
            {
                return null;
            }

            success = true;
            return converter.ConvertToInvariantString(value);
        }

        private static string ConvertUsingCustomConverters(IConverterProvider converterProvider, IResponseInfo response, object value)
        {
            var converter = converterProvider.FindBestOutputConverter(value.GetType(), response);
            if (converter == null)
            {
                return null;
            }

            string result = null;
            converter.ConvertFrom(value.GetType(), value, response);
            using (var reader = new StreamReader(response.Body))
            {
                result = reader.ReadToEnd();
            }

            response.Body.Seek(0, SeekOrigin.Begin);
            return result;
        }
    }
}