using System.Reflection;
using URSA.ComponentModel;
using URSA.Web.Converters;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description.Entities;

namespace URSA.Web.Http.Description
{
    /// <summary>Registers custom components.</summary>
    public class ComponentInstaller : IComponentInstaller
    {
        /// <inheritdoc />
        public void InstallComponents(IComponentProviderBuilder componentProviderBuilder, IComponentProvider componentProvider)
        {
            if (componentProvider.IsRoot)
            {
                return;
            }

            componentProviderBuilder.Register<IHypermediaFacilityFactory, HypermediaFacilityFactory>(() => new HypermediaFacilityFactory(
                () => componentProvider.Resolve<IEntityContextProvider>(),
                type => (IHttpControllerDescriptionBuilder)componentProvider.Resolve(type),
                type => (IApiDescriptionBuilder)componentProvider.Resolve(type)));
            componentProviderBuilder.RegisterAll<IConverter>(new[] { GetType().GetTypeInfo().Assembly }, Lifestyles.Transient);
        }
    }
}
