using System;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Provides a description of a result binding facility.</summary>
    public interface IResultBinder
    {
        /// <summary>Binds the results.</summary>
        /// <typeparam name="TResult">Type of the primary result object expected.</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>Array of objects being a result of a given response.</returns>
        object[] BindResults<TResult>(IRequestInfo request);

        /// <summary>Binds the results.</summary>
        /// <param name="primaryResultType">Type of the primary result object expected.</param>
        /// <param name="request">The request.</param>
        /// <returns>Array of objects being a result of a given response.</returns>
        object[] BindResults(Type primaryResultType, IRequestInfo request);
    }

    /// <summary>Provides a description of a result binding facility.</summary>
    /// <typeparam name="T">Type of the request handled.</typeparam>
    public interface IResultBinder<T> : IResultBinder where T : IRequestInfo
    {
        /// <summary>Binds the results.</summary>
        /// <typeparam name="TResult">Type of the primary result object expected.</typeparam>
        /// <param name="requestInfo">The request.</param>
        /// <returns>Array of objects being a result of a given response.</returns>
        object[] BindResults<TResult>(T requestInfo);

        /// <summary>Binds the results.</summary>
        /// <param name="primaryResultType">Type of the primary result object expected.</param>
        /// <param name="requestInfo">The request.</param>
        /// <returns>Array of objects being a result of a given response.</returns>
        object[] BindResults(Type primaryResultType, T requestInfo);
    }
}