using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace URSA.Web.Http
{
    /// <summary>Describes an HTTP request.</summary>
    public sealed class RequestInfo : IRequestInfo, IDisposable
    {
        private Stream _stream;

        /// <summary>Initializes a new instance of the <see cref="RequestInfo"/> class.</summary>
        /// <param name="method">HTTP method verb of the request.</param>
        /// <param name="uri">Address requested.</param>
        /// <param name="body">Body of the request.</param>
        /// <param name="headers">Headers of the request.</param>
        public RequestInfo(Verb method, Uri uri, Stream body, params Header[] headers)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            Method = method;
            Uri = uri;
            Body = new UnclosableStream(_stream = body);
            Headers = new HeaderCollection();
            headers.ForEach(header => Headers.Add(header));
        }

        /// <summary>Initializes a new instance of the <see cref="RequestInfo"/> class.</summary>
        /// <param name="method">HTTP method verb of the request.</param>
        /// <param name="uri">Address requested.</param>
        /// <param name="body">Body of the request.</param>
        /// <param name="headers">Headers of the request.</param>
        public RequestInfo(Verb method, Uri uri, Stream body, HeaderCollection headers)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            Method = method;
            Uri = uri;
            Body = new UnclosableStream(_stream = body);
            Headers = headers ?? new HeaderCollection();
        }

        /// <summary>Gets the HTTP method verb of the request.</summary>
        public Verb Method { get; private set; }

        /// <inheritdoc />
        public Stream Body { get; private set; }

        /// <inheritdoc />
        public Uri Uri { get; private set; }

        /// <summary>Gets the request headers.</summary>
        public HeaderCollection Headers { get; private set; }

        /// <inheritdoc />
        IDictionary<string, string> IRequestInfo.Headers { get { return Headers; } }

        /// <summary>Parses a given string as a <see cref="RequestInfo" />.</summary>
        /// <param name="method">Method of the request.</param>
        /// <param name="uri">Uri of the request.</param>
        /// <param name="message">Request content.</param>
        /// <returns>Instance of the <see cref="RequestInfo" />.</returns>
        public static RequestInfo Parse(Verb method, Uri uri, string message)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (message.Length == 0)
            {
                throw new ArgumentOutOfRangeException("message");
            }

            string[] parts = Regex.Split(message, "\r\n\r\n");
            Encoding encoding = Encoding.UTF8;
            HeaderCollection headers = HeaderCollection.Parse(parts[0]);
            return new RequestInfo(method, uri, (parts.Length > 1 ? new MemoryStream(encoding.GetBytes(parts[1].Trim('\r', '\n'))) : new MemoryStream()), headers);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}