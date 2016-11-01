using System.Linq;
using System.Reflection;
using URSA.Configuration;

namespace URSA.ComponentModel
{
    /// <summary>Exposes useful <see cref="IComponentComposer" /> extension methods.</summary>
    public static class ComponentProviderBuilderExtensions
    {
        /// <summary>Auto-discovers <see cref="IComponentInstaller" /> implementing types in the assemblies scanned by the <see cref="IComponentProvider" /> initialization routine.</summary>
        /// <param name="componentComposer">Component provider builder to be used for registrations.</param>
        public static void InstallComponents(this IComponentComposer componentComposer)
        {
            foreach (var assembly in UrsaConfigurationSection.GetInstallerAssemblies())
            {
                try
                {
                    var installerTypes = from type in assembly.ExportedTypes
                                         where (typeof(IComponentInstaller).IsAssignableFrom(type)) && (!type.GetTypeInfo().IsAbstract)
                                         from ctor in type.GetConstructors()
                                         where ctor.GetParameters().Length == 0
                                         select ctor;
                    foreach (var type in installerTypes)
                    {
                        ((IComponentInstaller)type.Invoke(null)).InstallComponents(componentComposer);
                    }
                }
                catch
                {
                    // Suppress any failed assemblies.
                }
            }
        }
    }
}
