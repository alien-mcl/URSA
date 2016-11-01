using System.Reflection;
using URSA.ComponentModel;
using URSA.Web.Converters;

namespace URSA.Web.Http
{
    /// <summary>Registers custom components.</summary>
    public class ComponentInstaller : IComponentInstaller
    {
        /// <inheritdoc />
        public void InstallComponents(IComponentComposer componentComposer)
        {
            componentComposer.RegisterAll<IConverter>(new[] { GetType().GetTypeInfo().Assembly }, Lifestyles.Singleton);
        }
    }
}
