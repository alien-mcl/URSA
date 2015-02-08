using System.Net;
using URSA.Web.Http;

namespace System
{
    /// <summary>Provides useful exception extension methods.</summary>
    public static class ExceptionExtensions
    {
        /// <summary>Converts several standard exceptions into their corresponding HTTP equivalents.</summary>
        /// <param name="exception">Exception to be mapped.</param>
        /// <returns>Instance of the <see cref="ProtocolException"/> with a proper HTTP status code.</returns>
        public static ProtocolException AsHttpException(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            switch (exception.GetType().FullName)
            {
                case "System.Web.HttpException":
                    return (ProtocolException)exception;
                case "System.ArgumentException":
                case "System.ArgumentNullException":
                case "System.ArgumentOutOfRangeException":
                    return new ProtocolException(HttpStatusCode.BadRequest, exception);
                case "System.NotImplementedException":
                    return new ProtocolException(HttpStatusCode.NotImplemented, exception);
                case "System.UnauthorizedAccessException":
                    return new ProtocolException(HttpStatusCode.Unauthorized, exception);
                default:
                    return new ProtocolException(HttpStatusCode.InternalServerError, exception);
            }
        }
    }
}