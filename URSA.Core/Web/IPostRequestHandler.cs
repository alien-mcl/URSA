using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Provides a basic description of the post-request event handler.</summary>
    public interface IPostRequestHandler
    {
        /// <summary>Processes the specified request after execution.</summary>
        /// <param name="responseInfo">The response information.</param>
        /// <returns>Task of this processing.</returns>
        Task Process(IResponseInfo responseInfo);
    }
}