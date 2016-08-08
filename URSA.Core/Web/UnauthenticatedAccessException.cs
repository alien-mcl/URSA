using System;
using System.Diagnostics.CodeAnalysis;
#if !CORE
using System.Runtime.Serialization;
#endif

namespace URSA.Web
{
    /// <summary>Exception related to access to a resource by an unauthenticated identity.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
#if !CORE
    [Serializable]
#endif
    public class UnauthenticatedAccessException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="UnauthenticatedAccessException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public UnauthenticatedAccessException(string message) : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UnauthenticatedAccessException"/> class.</summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public UnauthenticatedAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }

#if !CORE
        /// <summary>Initializes a new instance of the <see cref="UnauthenticatedAccessException"/> class.</summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <param name="streamingContext">The streaming context.</param>
        public UnauthenticatedAccessException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
#endif
    }
}