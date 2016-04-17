using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Converters;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Provides an implementation of the <see cref="IResultBinder" />.</summary>
    public class ResultBinder : IResultBinder<RequestInfo>
    {
        private readonly IConverterProvider _converterProvider;

        /// <summary>Initializes a new instance of the <see cref="ResultBinder"/> class.</summary>
        /// <param name="converterProvider">The converter provider.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public ResultBinder(IConverterProvider converterProvider)
        {
            if (converterProvider == null)
            {
                throw new ArgumentNullException("converterProvider");
            }

            _converterProvider = converterProvider;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        object[] IResultBinder.BindResults<T>(IRequestInfo request)
        {
            if (!(request is RequestInfo))
            {
                throw new ArgumentOutOfRangeException("request");
            }

            return BindResults<T>((RequestInfo)request);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        object[] IResultBinder.BindResults(Type primaryResultType, IRequestInfo request)
        {
            if (!(request is RequestInfo))
            {
                throw new ArgumentOutOfRangeException("request");
            }

            return BindResults(primaryResultType, (RequestInfo)request);
        }

        /// <inheritdoc />
        public object[] BindResults<T>(RequestInfo requestInfo)
        {
            return BindResults(typeof(T), requestInfo);
        }

        /// <inheritdoc />
        public object[] BindResults(Type primaryResultType, RequestInfo requestInfo)
        {
            if (primaryResultType == null)
            {
                throw new ArgumentNullException("primaryResultType");
            }

            if (requestInfo == null)
            {
                throw new ArgumentNullException("requestInfo");
            }

            var result = CheckHeader(primaryResultType, requestInfo);
            var converter = _converterProvider.FindBestInputConverter(primaryResultType, requestInfo);
            if (converter == null)
            {
                return result.ToArray();
            }

            var value = converter.ConvertTo(primaryResultType, requestInfo);
            if (value != null)
            {
                result.Add(value);
            }

            return result.ToArray();
        }

        private IList<object> CheckHeader(Type primaryResultType, RequestInfo requestInfo)
        {
            var result = new List<object>();
            if (String.IsNullOrEmpty(requestInfo.Headers.Location))
            {
                return result;
            }

            var converter = TypeDescriptor.GetConverter(primaryResultType);
            if ((converter == null) || (!converter.CanConvertFrom(typeof(string))))
            {
                return result;
            }

            var segments = (requestInfo.Headers.Location.StartsWith("http") ?
                new Uri(requestInfo.Headers.Location).Segments.Select(segment => segment.Trim('/')).Where(segment => segment.Length > 0).ToArray() :
                requestInfo.Headers.Location.Split('/'));
            for (int index = segments.Length - 1; index >= 0; index--)
            {
                var segment = segments[index];
                try
                {
                    var value = converter.ConvertFromInvariantString(segment);
                    if (value == null)
                    {
                        continue;
                    }

                    result.Add(value);
                    break;
                }
                catch
                {
                }
            }

            return result;
        }
    }
}