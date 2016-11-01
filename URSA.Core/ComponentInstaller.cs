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
        public void InstallComponents(IComponentComposer componentComposer)
        {
            var controllerActivatorType = UrsaConfigurationSection.Default.ControllerActivatorType;
            var controllerActivatorCtor = UrsaConfigurationSection.GetProvider<IControllerActivator>(controllerActivatorType, typeof(IComponentResolver));
            componentComposer.Register(
                typeof(IControllerActivator),
                controllerActivatorType,
                context => controllerActivatorCtor.Invoke(new object[] { context }),
                Lifestyles.Scoped);

            var converterProviderType = UrsaConfigurationSection.Default.ConverterProviderType ?? typeof(DefaultConverterProvider);
            var converterProviderCtor = UrsaConfigurationSection.GetProvider<IConverterProvider>(converterProviderType);
            componentComposer.Register(
                typeof(IConverterProvider),
                converterProviderType,
                context =>
                    {
                        var result = (IConverterProvider)converterProviderCtor.Invoke(null);
                        result.Initialize(() => context.ResolveAll<IConverter>());
                        return result;
                    });
        }
    }
}
