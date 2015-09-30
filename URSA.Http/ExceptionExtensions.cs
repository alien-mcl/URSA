using System.Diagnostics.CodeAnalysis;
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
        [ExcludeFromCodeCoverage]
        public static ProtocolException AsHttpException(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            try
            {
                switch (exception.GetType().FullName)
                {
                    case "URSA.Web.NotFoundException":
                        throw new ProtocolException(HttpStatusCode.NotFound, exception);
                    case "URSA.Web.AlreadyExistsException":
                        throw new ProtocolException(HttpStatusCode.Conflict, exception);
                    case "URSA.Web.Http.ProtocolException":
                        return (ProtocolException)exception;
                    case "System.Web.HttpException":
                        throw new ProtocolException((HttpStatusCode)((Web.HttpException)exception).WebEventCode);
                    case "System.ArgumentException":
                    case "System.ArgumentNullException":
                    case "System.ArgumentOutOfRangeException":
                        throw new ProtocolException(HttpStatusCode.BadRequest, exception);
                    case "System.NotImplementedException":
                        throw new ProtocolException(HttpStatusCode.NotImplemented, exception);
                    case "System.UnauthorizedAccessException":
                        throw new ProtocolException(HttpStatusCode.Unauthorized, exception);
                    default:
                        throw new ProtocolException(HttpStatusCode.InternalServerError, exception);
                }
            }
            catch (ProtocolException result)
            {
                return result;
            }
        }
    }
}