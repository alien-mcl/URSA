using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP exception.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    [Serializable]
    public class ProtocolException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ProtocolException" /> class.</summary>
        /// <param name="status">An associated HTTP status code of this exception</param>
        public ProtocolException(HttpStatusCode status)
        {
            Status = status;
        }

        /// <summary>Initializes a new instance of the <see cref="ProtocolException" /> class.</summary>
        /// <param name="status">An associated HTTP status code of this exception</param>
        /// <param name="message">Message of the exception.</param>
        public ProtocolException(HttpStatusCode status, string message) : base(message)
        {
            Status = status;
        }

        /// <summary>Initializes a new instance of the <see cref="ProtocolException" /> class.</summary>
        /// <param name="status">An associated HTTP status code of this exception</param>
        /// <param name="innerException">Inner exception.</param>
        public ProtocolException(HttpStatusCode status, Exception innerException) : base(innerException.Message, innerException)
        {
            Status = status;
        }
        
        /// <summary>Initializes a new instance of the <see cref="ProtocolException" /> class.</summary>
        /// <param name="status">An associated HTTP status code of this exception</param>
        /// <param name="message">Message of the exception.</param>
        /// <param name="innerException">Inner exception.</param>
        public ProtocolException(HttpStatusCode status, string message, Exception innerException) : base(message, innerException)
        {
            Status = status;
        }

        /// <summary>Initializes a new instance of the <see cref="ProtocolException"/> class.</summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <param name="streamingContext">The streaming context.</param>
        public ProtocolException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        /// <summary>Gets the associated HTTP status code of this exception.</summary>
        public HttpStatusCode Status { get; private set; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            base.GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("Status", Status);
        }
    }
}