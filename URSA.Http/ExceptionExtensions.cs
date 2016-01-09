using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web;
using URSA.Web;
using URSA.Web.Http;

namespace System
{
    /// <summary>Provides useful exception extension methods.</summary>
    public static class ExceptionExtensions
    {
        private static readonly IDictionary<Type, HttpStatusCode> ExceptionHttpStatusCodeMap;

        static ExceptionExtensions()
        {
            ExceptionHttpStatusCodeMap = new ConcurrentDictionary<Type, HttpStatusCode>();
            ExceptionHttpStatusCodeMap[typeof(ArgumentException)] = HttpStatusCode.BadRequest;
            ExceptionHttpStatusCodeMap[typeof(NotFoundException)] = HttpStatusCode.NotFound;
            ExceptionHttpStatusCodeMap[typeof(AlreadyExistsException)] = HttpStatusCode.Conflict;
            ExceptionHttpStatusCodeMap[typeof(UnauthenticatedAccessException)] = HttpStatusCode.Unauthorized;
            ExceptionHttpStatusCodeMap[typeof(AccessDeniedException)] = HttpStatusCode.Forbidden;
            ExceptionHttpStatusCodeMap[typeof(NotImplementedException)] = HttpStatusCode.NotImplemented;
            ExceptionHttpStatusCodeMap[typeof(UnauthorizedAccessException)] = HttpStatusCode.Unauthorized;
        }

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

            if (exception is ProtocolException)
            {
                return (ProtocolException)exception;
            }

            HttpStatusCode httpStatusCode = (exception is HttpException ? (HttpStatusCode)((HttpException)exception).WebEventCode : ExceptionHttpStatusCodeMap.Contains(exception));
            try
            {
                throw new ProtocolException(httpStatusCode, exception);
            }
            catch (ProtocolException result)
            {
                return result;
            }
        }

        private static HttpStatusCode Contains(this IDictionary<Type, HttpStatusCode> exceptionHttpStatusCodeMap, Exception exception)
        {
            HttpStatusCode result;
            if (exceptionHttpStatusCodeMap.TryGetValue(exception.GetType(), out result))
            {
                return result;
            }

            foreach (var map in from statusMap in exceptionHttpStatusCodeMap where statusMap.Key.IsInstanceOfType(exception) select statusMap)
            {
                return map.Value;
            }

            return HttpStatusCode.InternalServerError;
        }
    }
}