using System.Collections.Generic;
using System.Reflection;
using URSA.Web.Description;

namespace URSA.Web
{
    /// <summary>Provides a basic description of the handler class method mapping facility.</summary>
    public interface IDelegateMapper
    {
        /// <summary>Maps a given requested uri to an actual method.</summary>
        /// <param name="request">Requested uri.</param>
        /// <returns><see cref="IRequestMapping" /> pointing to the target handler or <b>null</b>.</returns>
        IRequestMapping MapRequest(IRequestInfo request);

        /// <summary>Gets the Uri template for a given controller method.</summary>
        /// <param name="methodInfo">Method to retrieve template for.</param>
        /// <param name="parameterMapping">Resulting parameter mapping for any variable part of the template returned.</param>
        /// <returns>Uri template string or <b>null</b>.</returns>
        string GetMethodUriTemplate(MethodInfo methodInfo, out IEnumerable<ArgumentInfo> parameterMapping);
    }

    /// <summary>Provides a basic description of the handler class mapping facility.</summary>
    /// <typeparam name="T">Type of requests to be mapped.</typeparam>
    public interface IDelegateMapper<T> : IDelegateMapper where T : IRequestInfo
    {
        /// <summary>Maps a given requested uri to an actual method.</summary>
        /// <param name="request">Requested uri.</param>
        /// <returns><see cref="IRequestMapping" /> pointing to the target handler or <b>null</b>.</returns>
        IRequestMapping MapRequest(T request);
    }
}