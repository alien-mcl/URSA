using System.Threading.Tasks;

namespace URSA.Web
{
    /// <summary>Provides a basic description of the pre-request event handler.</summary>
    public interface IPreRequestHandler
    {
        /// <summary>Processes the specified request before it is executed.</summary>
        /// <param name="requestInfo">The request information.</param>
        /// <returns>Task of this processing.</returns>
        Task Process(IRequestInfo requestInfo);
    }
}