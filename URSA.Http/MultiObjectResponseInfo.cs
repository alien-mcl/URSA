using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using URSA.Web.Converters;

namespace URSA.Web.Http
{
    /// <summary>Describes a response with an multiple objects.</summary>
    public class MultiObjectResponseInfo : ResponseInfo
    {
        private Stream _body;

        /// <summary>Initializes a new instance of the <see cref="MultiObjectResponseInfo" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="values">Values to be returned.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        public MultiObjectResponseInfo(RequestInfo request, IEnumerable<object> values, IConverterProvider converterProvider, params Header[] headers) :
            this(Encoding.UTF8, request, values, converterProvider, headers)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MultiObjectResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="values">Values to be returned.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        public MultiObjectResponseInfo(Encoding encoding, RequestInfo request, IEnumerable<object> values, IConverterProvider converterProvider, params Header[] headers) :
            base(encoding, request, headers)
        {
            Initialize(encoding, request, values, converterProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="MultiObjectResponseInfo" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="values">Values to be returned.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        public MultiObjectResponseInfo(RequestInfo request, IEnumerable<object> values, IConverterProvider converterProvider, HeaderCollection headers) :
            this(Encoding.UTF8, request, values, converterProvider, headers)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MultiObjectResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="values">Values to be returned.</param>
        /// <param name="converterProvider">Converter provider instance.</param>
        /// <param name="headers">Headers of the response.</param>
        [ExcludeFromCodeCoverage]
        public MultiObjectResponseInfo(Encoding encoding, RequestInfo request, IEnumerable<object> values, IConverterProvider converterProvider, HeaderCollection headers) :
            base(encoding, request, headers)
        {
            Initialize(encoding, request, values, converterProvider);
        }

        /// <summary>Gets the object responses for each value passed to the constructor.</summary>
        public IEnumerable<ResponseInfo> ObjectResponses { get; private set; }

        /// <summary>Gets the values.</summary>
        public IEnumerable<object> Values { get { return ObjectResponses.Select(item => ((ObjectResponseInfo)item).Object); } } 

        /// <inheritdoc />
        public override sealed Stream Body { get; protected set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override sealed void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        private void Initialize(Encoding encoding, RequestInfo request, IEnumerable<object> values, IConverterProvider converterProvider)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            var boundary = Guid.NewGuid().ToString();
            Headers.ContentType = String.Format("multipart/mixed; boundary=\"{0}\"", boundary);
            _body = new MemoryStream();
            Body = new UnclosableStream(_body);
            var valueResponses = new List<ResponseInfo>();
            using (var writer = new StreamWriter(Body))
            {
                foreach (var value in values.Where(value => value != null))
                {
                    writer.Write("--{0}\r\n", boundary);
                    var objectResponse = ObjectResponseInfo<object>.CreateInstance(encoding, request, value.GetType(), value, converterProvider);
                    valueResponses.Add(objectResponse);
                    foreach (var header in objectResponse.Headers)
                    {
                        switch (header.Name)
                        {
                            case "Content-Lenght":
                                break;
                            default:
                                writer.Write("{0}\r\n", header);
                                break;
                        }
                    }

                    writer.Write("Content-Length:{0}\r\n\r\n", objectResponse.Body.Length);
                    writer.Flush();
                    objectResponse.Body.CopyTo(Body);
                    writer.Flush();
                    writer.Write("\r\n");
                    writer.Flush();
                }

                writer.Write("--{0}--", boundary);
                writer.Flush();
            }

            _body.Seek(0, SeekOrigin.Begin);
            ObjectResponses = valueResponses;
        }
    }
}