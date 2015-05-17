namespace URSA.Web.Http
{
    /// <summary>Defines a contract for response composition facility.</summary>
    public interface IResponseComposer
    {
        /// <summary>Composes the response.</summary>
        /// <param name="requestMapping">The request mapping.</param>
        /// <param name="output">The output.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>Instance of the <see cref="ResponseInfo" />.</returns>
        ResponseInfo ComposeResponse(IRequestMapping requestMapping, object output, params object[] arguments);
    }
}