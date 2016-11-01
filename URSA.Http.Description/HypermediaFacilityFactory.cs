using System;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Provides a basic implementation of the <see cref="IHypermediaFacilityFactory" />.</summary>
    public class HypermediaFacilityFactory : IHypermediaFacilityFactory
    {
        private readonly Func<IEntityContextProvider> _entityContextProviderFactoryMethod;
        private readonly Func<Type, IHttpControllerDescriptionBuilder> _controllerDescriptionBuilderFactoryMethod;
        private readonly Func<Type, IApiDescriptionBuilder> _apiDescriptionBuilderFactoryMethod;

        /// <summary>Initializes a new instance of the <see cref="HypermediaFacilityFactory" /> class.</summary>
        /// <param name="entityContextProviderFactoryMethod">Entity context provider factory method.</param>
        /// <param name="controllerDescriptionBuilderFactoryMethod">Controller description builder factory method.</param>
        /// <param name="apiDescriptionBuilderFactoryMethod">API description builder factory method,</param>
        public HypermediaFacilityFactory(
            Func<IEntityContextProvider> entityContextProviderFactoryMethod,
            Func<Type, IHttpControllerDescriptionBuilder> controllerDescriptionBuilderFactoryMethod,
            Func<Type, IApiDescriptionBuilder> apiDescriptionBuilderFactoryMethod)
        {
            if (entityContextProviderFactoryMethod == null)
            {
                throw new ArgumentNullException("entityContextProviderFactoryMethod");
            }

            if (controllerDescriptionBuilderFactoryMethod == null)
            {
                throw new ArgumentNullException("controllerDescriptionBuilderFactoryMethod");
            }

            if (apiDescriptionBuilderFactoryMethod == null)
            {
                throw new ArgumentNullException("apiDescriptionBuilderFactoryMethod");
            }

            _entityContextProviderFactoryMethod = entityContextProviderFactoryMethod;
            _controllerDescriptionBuilderFactoryMethod = controllerDescriptionBuilderFactoryMethod;
            _apiDescriptionBuilderFactoryMethod = apiDescriptionBuilderFactoryMethod;
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
                _entityContextProviderFactoryMethod(),
                _controllerDescriptionBuilderFactoryMethod(controller.GetType()),
                _apiDescriptionBuilderFactoryMethod(controller.GetType()));
        }
    }
}
