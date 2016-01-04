namespace URSA.Web
{
    /// <summary>Serves as an abstract of the main entry point for the URSA.</summary>
    /// <typeparam name="T">Type of the requests.</typeparam>
    /// <typeparam name="TR">Type of the response.</typeparam>
    public abstract class RequestHandlerBase<T, TR> : IRequestHandler<T, TR>
        where T : IRequestInfo
        where TR : IResponseInfo
    {
        /// <inheritdoc />
        public abstract TR HandleRequest(T request);
    }
}