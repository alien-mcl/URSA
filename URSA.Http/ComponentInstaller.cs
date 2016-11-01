using System.Reflection;
using URSA.ComponentModel;
using URSA.Web.Converters;

namespace URSA.Web.Http
{
    /// <summary>Registers custom components.</summary>
    public class ComponentInstaller : IComponentInstaller
    {
        /// <inheritdoc />
        public void InstallComponents(IComponentProviderBuilder componentProviderBuilder, IComponentProvider componentProvider)
        {
            if (componentProvider.IsRoot)
            {
                componentProviderBuilder.RegisterAll<IConverter>(new[] { GetType().GetTypeInfo().Assembly }, Lifestyles.Singleton);
            }
        }
    }
}
