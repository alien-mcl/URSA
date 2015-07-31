using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a contract of the API description building facility.</summary>
    /// <typeparam name="T">Type of the controller being described.</typeparam>
    public interface IApiDescriptionBuilder<T> where T : IController
    {
        /// <summary>Builds an API description.</summary>
        /// <param name="apiDocumentation">API documentation.</param>
        void BuildDescription(IApiDocumentation apiDocumentation);
    }
}