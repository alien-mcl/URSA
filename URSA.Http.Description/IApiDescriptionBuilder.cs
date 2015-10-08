using System;
using System.Collections.Generic;
using URSA.Web.Http.Description.Hydra;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a contract of the API description building facility.</summary>
    public interface IApiDescriptionBuilder
    {
        /// <summary>Builds an API description.</summary>
        /// <param name="apiDocumentation">API documentation.</param>
        /// <param name="profiles">Requested media type profiles if any.</param>
        void BuildDescription(IApiDocumentation apiDocumentation, IEnumerable<Uri> profiles);
    }

    /// <summary>Provides a contract of the API description building facility.</summary>
    /// <typeparam name="T">Type of the controller being described.</typeparam>
    public interface IApiDescriptionBuilder<T> : IApiDescriptionBuilder where T : IController
    {
    }
}