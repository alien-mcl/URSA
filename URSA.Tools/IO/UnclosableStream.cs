using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>Wraps streams so they cannot be closed.</summary>
    [ExcludeFromCodeCoverage]
    public class UnclosableStream : Stream, IDisposable
    {
        private readonly Stream _stream;

        /// <summary>Initializes a new instance of the <see cref="UnclosableStream" /> class.</summary>
        /// <param name="stream">Stream to be wrapped.</param>
        public UnclosableStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            _stream = stream;
        }

        /// <inheritdoc />
        public override bool CanRead { get { return _stream.CanRead; } }

        /// <inheritdoc />
        public override bool CanSeek { get { return _stream.CanSeek; } }

        /// <inheritdoc />
        public override bool CanWrite { get { return _stream.CanWrite; } }

        /// <inheritdoc />
        public override long Length { get { return _stream.Length; } }

        /// <inheritdoc />
        public override long Position { get { return _stream.Position; } set { _stream.Position = value; } }

        /// <inheritdoc />
        public override bool CanTimeout { get { return _stream.CanTimeout; } }

        /// <inheritdoc />
        public override int ReadTimeout { get { return _stream.ReadTimeout; } set { _stream.ReadTimeout = value; } }

        /// <inheritdoc />
        public override int WriteTimeout { get { return _stream.WriteTimeout; } set { _stream.WriteTimeout = value; } }

        /// <inheritdoc />
        public new void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            _stream.Flush();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _stream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc />
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <inheritdoc />
        public override ObjRef CreateObjRef(Type requestedType)
        {
            return _stream.CreateObjRef(requestedType);
        }

        /// <inheritdoc />
        public override int EndRead(IAsyncResult asyncResult)
        {
            return _stream.EndRead(asyncResult);
        }

        /// <inheritdoc />
        public override void EndWrite(IAsyncResult asyncResult)
        {
            _stream.EndWrite(asyncResult);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return _stream.Equals(obj);
        }

        /// <inheritdoc />
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _stream.FlushAsync(cancellationToken);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _stream.GetHashCode();
        }

        /// <inheritdoc />
        public override object InitializeLifetimeService()
        {
            return _stream.InitializeLifetimeService();
        }

        /// <inheritdoc />
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override int ReadByte()
        {
            return _stream.ReadByte();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _stream.ToString();
        }

        /// <inheritdoc />
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc />
        public override void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
        }
    }
}