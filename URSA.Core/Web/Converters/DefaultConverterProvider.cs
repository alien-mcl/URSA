using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace URSA.Web.Converters
{
    /// <summary>Default implementation of the <see cref="IConverterProvider" />.</summary>
    public class DefaultConverterProvider : IConverterProvider
    {
        private IEnumerable<IConverter> _converters;

        /// <inheritdoc />
        public void Initialize(IEnumerable<IConverter> converters)
        {
            if (converters == null)
            {
                throw new ArgumentNullException("converters");
            }

            _converters = converters;
        }

        /// <inheritdoc />
        public IConverter FindBestInputConverter<T>(IRequestInfo request, bool ignoreProtocol = false)
        {
            return FindBestInputConverter(typeof(T), request, ignoreProtocol);
        }

        /// <inheritdoc />
        public IConverter FindBestInputConverter(Type expectedType, IRequestInfo request, bool ignoreProtocol = false)
        {
            if (_converters == null)
            {
                throw new InvalidOperationException("Default converter provider is not initialized.");
            }

            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            return (from item in _converters
                    let level = item.CanConvertTo(expectedType, request)
                    where ((ignoreProtocol) || ((!ignoreProtocol) &&
                        ((level & CompatibilityLevel.ProtocolMatch) == CompatibilityLevel.ProtocolMatch))) &&
                        ((level & CompatibilityLevel.TypeMatch) == CompatibilityLevel.TypeMatch)
                    let value = (ignoreProtocol ? level & ~CompatibilityLevel.ExactProtocolMatch : level)
                    orderby value descending
                    orderby (expectedType.GetItemType() != typeof(string) ? 1 : 0) descending
                    select item).FirstOrDefault();
        }

        /// <inheritdoc />
        public IConverter FindBestOutputConverter<T>(IResponseInfo response)
        {
            return FindBestOutputConverter(typeof(T), response);
        }

        /// <inheritdoc />
        public IConverter FindBestOutputConverter(Type expectedType, IResponseInfo response)
        {
            if (_converters == null)
            {
                throw new InvalidOperationException("Default converter provider is not initialized.");
            }

            if (expectedType == null)
            {
                throw new ArgumentNullException("expectedType");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            var result = (from item in _converters
                          let level = item.CanConvertFrom(expectedType, response)
                          where ((level & CompatibilityLevel.ProtocolMatch) == CompatibilityLevel.ProtocolMatch) &&
                              ((level & CompatibilityLevel.TypeMatch) == CompatibilityLevel.TypeMatch)
                          orderby level descending
                          orderby (expectedType.GetItemType() != typeof(string) ? 1 : 0) descending
                          select item).FirstOrDefault();
            if (result != null)
            {
                return result;
            }

            if (response.Request.OutputNeutral)
            {
                return (from item in _converters
                        let level = item.CanConvertTo(expectedType, response.Request)
                        orderby level descending
                        select item).FirstOrDefault();
            }

            return null;
        }
    }
}