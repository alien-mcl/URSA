using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using URSA.Web.Converters;
using URSA.Web.Http.Converters;

namespace URSA.Web.Http
{
    /// <summary>Describes an exception response.</summary>
    [ExcludeFromCodeCoverage]
    public class ExceptionResponseInfo : ObjectResponseInfo<Exception>
    {
        private static readonly IConverterProvider ConverterProvider;

        static ExceptionResponseInfo()
        {
            (ConverterProvider = new DefaultConverterProvider()).Initialize(new IConverter[] { new ExceptionConverter() });
        }

        /// <summary>Initializes a new instance of the <see cref="ExceptionResponseInfo" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="exception">Value object.</param>
        public ExceptionResponseInfo(RequestInfo request, Exception exception) : this(Encoding.UTF8, request, exception)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExceptionResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="exception">Value object.</param>
        public ExceptionResponseInfo(Encoding encoding, RequestInfo request, Exception exception) : base(encoding, request, exception, ConverterProvider)
        {
        }
    }
}