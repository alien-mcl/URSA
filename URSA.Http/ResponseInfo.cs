using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Describes an abstract HTTP response.</summary>
    public abstract class ResponseInfo : IResponseInfo
    {
        private HeaderCollection _headers;

        /// <summary>Initialies a new instance of the <see cref="ResponseInfo" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="headers">Headers of the response.</param>
        public ResponseInfo(RequestInfo request, params Header[] headers) : this(Encoding.UTF8, request, headers)
        {
        }

        /// <summary>Initialies a new instance of the <see cref="ResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="headers">Headers of the response.</param>
        public ResponseInfo(Encoding encoding, RequestInfo request, params Header[] headers)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Encoding = encoding;
            Request = request;
            _headers = new HeaderCollection();
            headers.ForEach(header => _headers.Add(header));
        }

        /// <summary>Initialies a new instance of the <see cref="ResponseInfo" /> class.</summary>
        /// <param name="request">Corresponding request.</param>
        /// <param name="headers">Headers of the response.</param>
        public ResponseInfo(RequestInfo request, HeaderCollection headers) : this(Encoding.UTF8, request, headers)
        {
        }

        /// <summary>Initialies a new instance of the <see cref="ResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="request">Corresponding request.</param>
        /// <param name="headers">Headers of the response.</param>
        public ResponseInfo(Encoding encoding, RequestInfo request, HeaderCollection headers)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            Encoding = encoding;
            Request = request;
            _headers = headers ?? new HeaderCollection();
        }

        /// <summary>Gets the text encoding of this response.</summary>
        public Encoding Encoding { get; private set; }

        /// <summary>Gets the corresponding request.</summary>
        public RequestInfo Request { get; private set; }

        /// <summary>Gets or sets the HTTP response status.</summary>
        public HttpStatusCode Status { get; set; }

        /// <inheritdoc />
        IRequestInfo IResponseInfo.Request { get { return Request; } }

        /// <summary>Gets the response headers.</summary>
        public HeaderCollection Headers { get { return _headers; } }

        /// <inheritdoc />
        IDictionary<string, string> IResponseInfo.Headers { get { return _headers; } }

        /// <inheritdoc />
        public abstract Stream Body { get; protected set; }

        /// <inheritdoc />
        public abstract void Dispose();
    }
}