using System;
using System.Net;
using System.Runtime.Serialization;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP exception.</summary>
    [Serializable]
    public class ProtocolException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ProtocolException" /> class.</summary>
        /// <param name="status">An associated HTTP status code of this exception</param>
        public ProtocolException(HttpStatusCode status) : base()
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

        /// <summary>Gets the associated HTTP status code of this exception.</summary>
        public HttpStatusCode Status { get; private set; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}