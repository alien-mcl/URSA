using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
#if !CORE
using System.Web;
#endif
using URSA.Web;
using URSA.Web.Http;

namespace System
{
    /// <summary>Provides useful exception extension methods.</summary>
    public static class ExceptionExtensions
    {
        private static readonly IDictionary<Type, HttpStatusCode> ExceptionMap;

        static ExceptionExtensions()
        {
            ExceptionMap = new ConcurrentDictionary<Type, HttpStatusCode>();
            ExceptionMap[typeof(ArgumentException)] = HttpStatusCode.BadRequest;
            ExceptionMap[typeof(ArgumentNullException)] = HttpStatusCode.BadRequest;
            ExceptionMap[typeof(ArgumentOutOfRangeException)] = HttpStatusCode.BadRequest;
            ExceptionMap[typeof(NotFoundException)] = HttpStatusCode.NotFound;
            ExceptionMap[typeof(AlreadyExistsException)] = HttpStatusCode.Conflict;
            ExceptionMap[typeof(UnauthenticatedAccessException)] = HttpStatusCode.Unauthorized;
            ExceptionMap[typeof(AccessDeniedException)] = HttpStatusCode.Forbidden;
            ExceptionMap[typeof(NotImplementedException)] = HttpStatusCode.NotImplemented;
            ExceptionMap[typeof(UnauthorizedAccessException)] = HttpStatusCode.Unauthorized;
        }

        /// <summary>Converts several standard exceptions into their corresponding HTTP equivalents.</summary>
        /// <param name="exception">Exception to be mapped.</param>
        /// <returns>Instance of the <see cref="ProtocolException"/> with a proper HTTP status code.</returns>
        public static ProtocolException AsHttpException(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            exception = exception.UnwrapException();
            if (exception is ProtocolException)
            {
                return (ProtocolException)exception;
            }

#if CORE
            HttpStatusCode httpStatusCode = exception.ToHttpStatusCode();
#else
            HttpStatusCode httpStatusCode = (exception is HttpException ? (HttpStatusCode)((HttpException)exception).GetHttpCode() : exception.ToHttpStatusCode());
#endif
            try
            {
                throw new ProtocolException(httpStatusCode, exception);
            }
            catch (ProtocolException result)
            {
                return result;
            }
        }

        /// <summary>Returns an HTTP status code for a given <paramref name="exception" />.</summary>
        /// <param name="exception">The exception.</param>
        /// <returns><see cref="HttpStatusCode" /> for a given <paramref name="exception" />.</returns>
        public static HttpStatusCode ToHttpStatusCode(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            return exception.GetType().ToHttpStatusCode();
        }

        /// <summary>Returns an HTTP status code for a given <paramref name="exceptionType" />.</summary>
        /// <param name="exceptionType">Type of the exception.</param>
        /// <returns><see cref="HttpStatusCode" /> for a given <paramref name="exceptionType" />.</returns>
        public static HttpStatusCode ToHttpStatusCode(this Type exceptionType)
        {
            if (exceptionType == null)
            {
                throw new ArgumentNullException("exceptionType");
            }

            if (!typeof(Exception).GetTypeInfo().IsAssignableFrom(exceptionType))
            {
                throw new ArgumentOutOfRangeException("exceptionType");
            }

            HttpStatusCode result;
            if (ExceptionMap.TryGetValue(exceptionType, out result))
            {
                return result;
            }

            foreach (var map in from statusMap in ExceptionMap where statusMap.Key.IsAssignableFrom(exceptionType) select statusMap)
            {
                return map.Value;
            }

            return HttpStatusCode.InternalServerError;
        }

        /// <summary>Returns an HTTP status code for a given <paramref name="exceptionTypeName" />.</summary>
        /// <param name="exceptionTypeName">Type name of the exception.</param>
        /// <returns><see cref="HttpStatusCode" /> for a given <paramref name="exceptionTypeName" />.</returns>
        public static HttpStatusCode ToHttpStatusCode(this string exceptionTypeName)
        {
            if (exceptionTypeName == null)
            {
                throw new ArgumentNullException("exceptionTypeName");
            }

            if ((exceptionTypeName.Length == 0) || (!exceptionTypeName.Contains("Exception")))
            {
                throw new ArgumentOutOfRangeException("exceptionTypeName");
            }

            foreach (var map in from statusMap in ExceptionMap where statusMap.Key.FullName == exceptionTypeName select statusMap)
            {
                return map.Value;
            }

            return HttpStatusCode.InternalServerError;
        }

        private static Exception UnwrapException(this Exception exception)
        {
            var targetInvocationException = exception as TargetInvocationException;
            if (targetInvocationException != null)
            {
                return targetInvocationException.InnerException.UnwrapException();
            }

            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                return (aggregateException.InnerExceptions.Count > 1 ? aggregateException : aggregateException.InnerException).UnwrapException();
            }

            return exception;
        }
    }
}