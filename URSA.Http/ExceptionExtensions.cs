using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
            ExceptionHttpStatusCodeMap[typeof(NotFoundException)] = HttpStatusCode.NotFound;
            ExceptionHttpStatusCodeMap[typeof(AlreadyExistsException)] = HttpStatusCode.Conflict;
            ExceptionHttpStatusCodeMap[typeof(ArgumentException)] = HttpStatusCode.BadRequest;
            ExceptionHttpStatusCodeMap[typeof(ArgumentNullException)] = HttpStatusCode.BadRequest;
            ExceptionHttpStatusCodeMap[typeof(ArgumentOutOfRangeException)] = HttpStatusCode.BadRequest;
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

            HttpStatusCode httpStatusCode = (exception is HttpException ? (HttpStatusCode)((HttpException)exception).WebEventCode : HttpStatusCode.InternalServerError);
            if (ExceptionHttpStatusCodeMap.ContainsKey(exception.GetType()))
            {
                httpStatusCode = ExceptionHttpStatusCodeMap[exception.GetType()];
            }

            try
            {
                throw new ProtocolException(httpStatusCode, exception);
            }
            catch (ProtocolException result)
            {
                return result;
            }
        }
    }
}