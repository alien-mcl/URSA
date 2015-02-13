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