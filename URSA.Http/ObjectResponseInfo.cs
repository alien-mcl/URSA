using System;
using System.IO;
using System.Text;
using URSA.Web.Converters;

namespace URSA.Web.Http
{
    /// <summary>Describes a response with an object.</summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    public class ObjectResponseInfo<T> : ResponseInfo
    {
        private Stream _body;

        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        public ObjectResponseInfo(RequestInfo request, T value, IConverterProvider converterProvider, params Header[] headers) : this(Encoding.UTF8, request, value, converterProvider, headers)
        {
        }
        
        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        public ObjectResponseInfo(Encoding encoding, RequestInfo request, T value, IConverterProvider converterProvider, params Header[] headers) : base(encoding, request, headers)
        {
            Initialize(value, converterProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        public ObjectResponseInfo(RequestInfo request, T value, IConverterProvider converterProvider, HeaderCollection headers) : this(Encoding.UTF8, request, value, converterProvider, headers)
        {
        }
        
        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        public ObjectResponseInfo(Encoding encoding, RequestInfo request, T value, IConverterProvider converterProvider, HeaderCollection headers) : base(encoding, request, headers)
        {
            Initialize(value, converterProvider);
        }

        /// <summary>Gets the value object.</summary>
        public T Value { get; private set; }

        /// <inheritdoc />
        public sealed override Stream Body { get; protected set; }

        /// <inheritdoc />
        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _body.Dispose();
                _body = null;
            }
        }

        private void Initialize(T value, IConverterProvider converterProvider)
        {
            if (converterProvider == null)
            {
                throw new ArgumentNullException("converterProvider");
            }

            var converter = converterProvider.FindBestOutputConverter<T>(this);
            if (converter == null)
            {
                throw new InvalidOperationException("Cannot serialize response without a matching converter.");
            }

            _body = new MemoryStream();
            Body = new UnclosableStream(_body);
            converter.ConvertFrom<T>(Value = value, this);
            _body.Seek(0, SeekOrigin.Begin);
        }
    }
}