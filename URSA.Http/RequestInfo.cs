using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using URSA.Security;

namespace URSA.Web.Http
{
    /// <summary>Describes an HTTP request.</summary>
    public sealed class RequestInfo : IRequestInfo, IDisposable
    {
        private const string AnyAny = "*/*";
        private readonly Stream _stream;
        private IClaimBasedIdentity _identity;

        /// <summary>Initializes a new instance of the <see cref="RequestInfo"/> class.</summary>
        /// <param name="method">HTTP method verb of the request.</param>
        /// <param name="uri">Address requested.</param>
        /// <param name="body">Body of the request.</param>
        /// <param name="identity">Identity of this request.</param>
        /// <param name="headers">Headers of the request.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public RequestInfo(Verb method, Uri uri, Stream body, IClaimBasedIdentity identity, params Header[] headers) :
            this(method, uri, body, identity, new HeaderCollection(headers))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RequestInfo"/> class.</summary>
        /// <param name="method">HTTP method verb of the request.</param>
        /// <param name="uri">Address requested.</param>
        /// <param name="body">Body of the request.</param>
        /// <param name="identity">Identity of this request.</param>
        /// <param name="headers">Headers of the request.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public RequestInfo(Verb method, Uri uri, Stream body, IClaimBasedIdentity identity, HeaderCollection headers)
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

            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            Method = method;
            Uri = uri;
            Body = new UnclosableStream(_stream = body);
            _identity = identity;
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
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public IClaimBasedIdentity Identity
        {
            get
            {
                return _identity;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                _identity = value;
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public bool OutputNeutral { get { return (Headers.Accept.IndexOf(AnyAny) != -1); } }

        /// <summary>Gets a value indicating whether this request is cross-origin resource sharing preflight request.</summary>
        public bool IsCorsPreflight
        {
            get { return ((Method == Verb.OPTIONS) && (!String.IsNullOrEmpty(Headers.Origin)) && (!String.IsNullOrEmpty(Headers.AccessControlRequestMethod))); }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
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
            return new RequestInfo(method, uri, (parts.Length > 1 ? new MemoryStream(encoding.GetBytes(parts[1].Trim('\r', '\n'))) : new MemoryStream()), new BasicClaimBasedIdentity(), headers);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}