using System;
using RDeF.Entities;
using URSA.Web.Description.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Description.Entities;

//// TODO: Check if the IHttpServerConfiguration can be resolved directly.
namespace URSA.Web.Http.Description
{
    /// <summary>Provides a basic implementation of the <see cref="IHypermediaFacilityFactory" />.</summary>
    public class HypermediaFacilityFactory : IHypermediaFacilityFactory
    {
        private readonly Func<IEntityContext> _entityContextFactoryMethod;
        private readonly Func<Type, IHttpControllerDescriptionBuilder> _controllerDescriptionBuilderFactoryMethod;
        private readonly Func<Type, IApiDescriptionBuilder> _apiDescriptionBuilderFactoryMethod;
        private readonly Func<IHttpServerConfiguration> _httpServerConfigurationFactoryMethod;

        /// <summary>Initializes a new instance of the <see cref="HypermediaFacilityFactory" /> class.</summary>
        /// <param name="entityContextFactoryMethod">Entity context provider factory method.</param>
        /// <param name="controllerDescriptionBuilderFactoryMethod">Controller description builder factory method.</param>
        /// <param name="apiDescriptionBuilderFactoryMethod">API description builder factory method,</param>
        /// <param name="httpServerConfigurationFactoryMethod">HTTP server configuration with base Uri factory method,</param>
        public HypermediaFacilityFactory(
            Func<IEntityContext> entityContextFactoryMethod,
            Func<Type, IHttpControllerDescriptionBuilder> controllerDescriptionBuilderFactoryMethod,
            Func<Type, IApiDescriptionBuilder> apiDescriptionBuilderFactoryMethod,
            Func<IHttpServerConfiguration> httpServerConfigurationFactoryMethod)
        {
            if (entityContextFactoryMethod == null)
            {
                throw new ArgumentNullException("entityContextFactoryMethod");
            }

            if (controllerDescriptionBuilderFactoryMethod == null)
            {
                throw new ArgumentNullException("controllerDescriptionBuilderFactoryMethod");
            }

            if (apiDescriptionBuilderFactoryMethod == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilderFactoryMethod");
            }

            if (httpServerConfigurationFactoryMethod == null)
            {
                throw new ArgumentNullException("httpServerConfigurationFactoryMethod");
            }

            _entityContextFactoryMethod = entityContextFactoryMethod;
            _controllerDescriptionBuilderFactoryMethod = controllerDescriptionBuilderFactoryMethod;
            _apiDescriptionBuilderFactoryMethod = apiDescriptionBuilderFactoryMethod;
            _httpServerConfigurationFactoryMethod = httpServerConfigurationFactoryMethod;
        }

        /// <inheritdoc />
        public IHypermediaFacility CreateFor(IController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            return new HypermediaFacility(
                controller,
                _entityContextFactoryMethod(),
                _controllerDescriptionBuilderFactoryMethod(controller.GetType()),
                _apiDescriptionBuilderFactoryMethod(controller.GetType()),
                _httpServerConfigurationFactoryMethod());
        }
    }
}
