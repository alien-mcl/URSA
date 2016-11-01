using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web;
using URSA.Web.Converters;

namespace URSA
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

            var controllerActivatorCtor = UrsaConfigurationSection.GetProvider<IControllerActivator>(
                UrsaConfigurationSection.Default.ControllerActivatorType, typeof(IComponentProvider));
            componentProviderBuilder.Register(
                typeof(IControllerActivator),
                UrsaConfigurationSection.Default.ControllerActivatorType,
                () => controllerActivatorCtor.Invoke(new object[] { componentProvider }));
            var converterProvider = (IConverterProvider)(UrsaConfigurationSection.GetProvider<IConverterProvider>(
                UrsaConfigurationSection.Default.ConverterProviderType ?? typeof(DefaultConverterProvider))).Invoke(null);
            converterProvider.Initialize(() => componentProvider.ResolveAll<IConverter>());
            componentProviderBuilder.Register(converterProvider);
        }
    }
}
