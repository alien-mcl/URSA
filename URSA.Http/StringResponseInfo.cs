using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace URSA.Web.Http
{
    /// <summary>Describes a string response.</summary>
    public class StringResponseInfo : ResponseInfo
    {
        private Stream _body;

        /// <summary>Initializes a new instance of the <see cref="StringResponseInfo" /> class.</summary>
        /// <param name="content">String content.</param>
        /// <param name="request">Request details.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public StringResponseInfo(string content, RequestInfo request) : this(Encoding.UTF8, content, request)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="StringResponseInfo" /> class.</summary>
        /// <param name="encoding">Text encoding of the response.</param>
        /// <param name="content">String content.</param>
        /// <param name="request">Request details.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public StringResponseInfo(Encoding encoding, string content, RequestInfo request) : base(encoding, request)
        {
            if (content != null)
            {
                _body = new MemoryStream(Encoding.GetBytes(Content = content));
            }
            else
            {
                Content = null;
                _body = new MemoryStream();
            }

            _body.Seek(0, SeekOrigin.Begin);
            Body = new UnclosableStream(_body);
        }

        /// <summary>Gets the string content of this response.</summary>
        public string Content { get; private set; }

        /// <inheritdoc />
        public sealed override Stream Body { get; protected set; }

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
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _body.Dispose();
            _body = null;
        }
    }
}