using System.Reflection;
using RDeF.Entities;
using URSA.ComponentModel;
using URSA.Web.Converters;
using URSA.Web.Description.Http;
using URSA.Web.Http.Configuration;

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
                    () => context.Resolve<IEntityContext>(),
                    type => (IHttpControllerDescriptionBuilder)context.Resolve(type),
                    type => (IApiDescriptionBuilder)context.Resolve(type),
                    () => context.Resolve<IHttpServerConfiguration>()));
            componentComposer.RegisterAll<IConverter>(new[] { GetType().GetTypeInfo().Assembly }, Lifestyles.Scoped);
        }
    }
}
