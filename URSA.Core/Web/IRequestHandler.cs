namespace URSA.Web
{
    /// <summary>Describes a request handler.</summary>
    /// <typeparam name="T">Type of requests to be handled.</typeparam>
    /// <typeparam name="R">Type of response returned.</typeparam>
    public interface IRequestHandler<T, R>
        where T : IRequestInfo
        where R : IResponseInfo
    {
        /// <summary>Handles the requests.</summary>
        /// <param name="request">Request details.</param>
        /// <returns>Resulting response.</returns>
        R HandleRequest(T request);
    }
}