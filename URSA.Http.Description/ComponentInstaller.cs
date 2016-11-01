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
        public void InstallComponents(IComponentComposer componentComposer)
        {
            componentComposer.Register<IHypermediaFacilityFactory, HypermediaFacilityFactory>(
                context => new HypermediaFacilityFactory(
                    () => context.Resolve<IEntityContextProvider>(),
                    type => (IHttpControllerDescriptionBuilder)context.Resolve(type),
                    type => (IApiDescriptionBuilder)context.Resolve(type)));
            componentComposer.RegisterAll<IConverter>(new[] { GetType().GetTypeInfo().Assembly }, Lifestyles.Scoped);
        }
    }
}
