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
    [ExcludeFromCodeCoverage]
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

            if (exception is ProtocolException)
            {
                return (ProtocolException)exception;
            }

            HttpStatusCode httpStatusCode = (exception is HttpException ? (HttpStatusCode)((HttpException)exception).WebEventCode : exception.ToHttpStatusCode());
            try
            {
                throw new ProtocolException(httpStatusCode, exception);
            }
            catch (ProtocolException result)
            {
                return result;
            }
        }

        /// <summary>Returns an HTTP status code for a given <typeparamref name="TException" /> type.</summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <returns><see cref="HttpStatusCode" /> for a given <typeparamref name="TException" /> type.</returns>
        public static HttpStatusCode GetHttpStatusCodeFor<TException>() where TException : Exception
        {
            return typeof(TException).ToHttpStatusCode();
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

            if (!typeof(Exception).IsAssignableFrom(exceptionType))
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
    }
}