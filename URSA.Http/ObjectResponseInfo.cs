using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using URSA.Web.Converters;

namespace URSA.Web.Http
{
    /// <summary>Abstract of an strongly typed object response.</summary>
    public abstract class ObjectResponseInfo : ResponseInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        protected ObjectResponseInfo(Encoding encoding, RequestInfo request, params Header[] headers) : base(encoding, request, headers)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        protected ObjectResponseInfo(Encoding encoding, RequestInfo request, HeaderCollection headers) : base(encoding, request, headers)
        {
        }

        /// <summary>Gets the value of this object response.</summary>
        public abstract object Object { get; }
    }

    /// <summary>Describes a response with an object.</summary>
    /// <typeparam name="T">Type of the value.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Suppression is OK - generic and non-generic class.")]
    public class ObjectResponseInfo<T> : ObjectResponseInfo
    {
        private Stream _body;

        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public ObjectResponseInfo(RequestInfo request, T value, IConverterProvider converterProvider, params Header[] headers)
            : this(Encoding.UTF8, request, value, converterProvider, headers)
        {
        }
        
        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public ObjectResponseInfo(Encoding encoding, RequestInfo request, T value, IConverterProvider converterProvider, params Header[] headers)
            : base(encoding, request, headers)
        {
            Initialize(value, converterProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public ObjectResponseInfo(RequestInfo request, T value, IConverterProvider converterProvider, HeaderCollection headers)
            : this(Encoding.UTF8, request, value, converterProvider, headers)
        {
        }
        
        /// <summary>Initializes a new instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="value">Value object.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public ObjectResponseInfo(Encoding encoding, RequestInfo request, T value, IConverterProvider converterProvider, HeaderCollection headers)
            : base(encoding, request, headers)
        {
            Initialize(value, converterProvider);
        }

        /// <summary>Gets the value object.</summary>
        public T Value { get; private set; }

        /// <inheritdoc />
        public override object Object { get { return Value; } }

        /// <inheritdoc />
        public sealed override Stream Body { get; protected set; }

        /// <summary>Creates the instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="request">The request.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <param name="value">The value.</param>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="headers">Optional headers.</param>
        /// <returns>Instance of the <see cref="ObjectResponseInfo{T}" />.</returns>
        public static ResponseInfo CreateInstance(Encoding encoding, RequestInfo request, Type valueType, object value, IConverterProvider converterProvider, HeaderCollection headers)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if ((value != null) && (!valueType.IsInstanceOfType(value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }

            return (ResponseInfo)typeof(ObjectResponseInfo<>).MakeGenericType(valueType)
                .GetConstructor(new[] { typeof(Encoding), typeof(RequestInfo), valueType, typeof(IConverterProvider), typeof(HeaderCollection) })
                .Invoke(new object[] { encoding, request, value, converterProvider, headers });
        }

        /// <summary>Creates the instance of the <see cref="ObjectResponseInfo{TTarget}" /> class.</summary>
        /// <typeparam name="TTarget">The type of the value.</typeparam>
        /// <param name="encoding">The encoding.</param>
        /// <param name="request">The request.</param>
        /// <param name="value">The value.</param>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="headers">Optional headers.</param>
        /// <returns>Instance of the <see cref="ObjectResponseInfo{TTarget}" />.</returns>
        public static ObjectResponseInfo<TTarget> CreateInstance<TTarget>(Encoding encoding, RequestInfo request, TTarget value, IConverterProvider converterProvider, HeaderCollection headers)
        {
            return (ObjectResponseInfo<TTarget>)CreateInstance(encoding, request, typeof(TTarget), value, converterProvider, headers);
        }

        /// <summary>Creates the instance of the <see cref="ObjectResponseInfo{T}" /> class.</summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="request">The request.</param>
        /// <param name="valueType">The type of the value.</param>
        /// <param name="value">The value.</param>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="headers">Optional headers.</param>
        /// <returns>Instance of the <see cref="ObjectResponseInfo{T}" />.</returns>
        public static ResponseInfo CreateInstance(Encoding encoding, RequestInfo request, Type valueType, object value, IConverterProvider converterProvider, params Header[] headers)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if ((value != null) && (!valueType.IsInstanceOfType(value)))
            {
                throw new ArgumentOutOfRangeException("value");
            }

            return (ResponseInfo)typeof(ObjectResponseInfo<>).MakeGenericType(valueType)
                .GetConstructor(new[] { typeof(Encoding), typeof(RequestInfo), valueType, typeof(IConverterProvider), typeof(Header[]) })
                .Invoke(new object[] { encoding, request, value, converterProvider, headers ?? new Header[0] });
        }

        /// <summary>Creates the instance of the <see cref="ObjectResponseInfo{TTarget}" /> class.</summary>
        /// <typeparam name="TTarget">The type of the value.</typeparam>
        /// <param name="encoding">The encoding.</param>
        /// <param name="request">The request.</param>
        /// <param name="value">The value.</param>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="headers">Optional headers.</param>
        /// <returns>Instance of the <see cref="ObjectResponseInfo{TTarget}" />.</returns>
        public static ObjectResponseInfo<TTarget> CreateInstance<TTarget>(Encoding encoding, RequestInfo request, TTarget value, IConverterProvider converterProvider, params Header[] headers)
        {
            return (ObjectResponseInfo<TTarget>)CreateInstance(encoding, request, typeof(TTarget), value, converterProvider, headers);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        [ExcludeFromCodeCoverage]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _body.Dispose();
            _body = null;
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
            converter.ConvertFrom(Value = value, this);
            _body.Flush();
            _body.Seek(0, SeekOrigin.Begin);
        }
    }
}