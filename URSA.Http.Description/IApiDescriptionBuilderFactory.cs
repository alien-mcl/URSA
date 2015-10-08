using System;

namespace URSA.Web.Http.Description
{
    /// <summary>Exposes an API description builder factory.</summary>
    public interface IApiDescriptionBuilderFactory
    {
        /// <summary>Creates this instance of the API description builder for given <paramref name="type" /> controller.</summary>
        /// <param name="type">Type of the controller for which to obtain a builder.</param>
        /// <returns>API description builder instance.</returns>
        IApiDescriptionBuilder Create(Type type);

        /// <summary>Creates this instance of the API description builder for <typeparamref name="T" /> controller.</summary>
        /// <typeparam name="T">Type of the controller for which to obtain a builder.</typeparam>
        /// <returns>API description builder instance.</returns>
        IApiDescriptionBuilder<T> Create<T>() where T : IController;
    }
}