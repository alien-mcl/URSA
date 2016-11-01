using URSA.ComponentModel;
using URSA.Http.AutoFac.Description;

namespace URSA.Web.Http.Description
{
    /// <summary>Registers custom components.</summary>
    public class ComponentInstaller : IComponentInstaller
    {
        /// <inheritdoc />
        public void InstallComponents(IComponentProviderBuilder componentProviderBuilder, IComponentProvider componentProvider)
        {
            ////componentProviderBuilder.Register<IApiDescriptionBuilderFactory, DefaultApiDescriptionBuilderFactory>(() => new DefaultApiDescriptionBuilderFactory(
            ////    type => (IApiDescriptionBuilder)componentProvider.Resolve(type)));
        }
    }
}
