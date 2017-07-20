using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
#if CORE
using System.Runtime.Loader;
#endif
using System.Text.RegularExpressions;
#if CORE
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
#endif
using URSA.ComponentModel;
using URSA.Web;
using URSA.Web.Converters;

namespace URSA.Configuration
{
    /// <summary>Describes the base URSA configuration section.</summary>
    public class UrsaConfigurationSection : ConfigurationSection
    {
        /// <summary>Defines a configuration section group name for URSA framework.</summary>
        public const string ConfigurationSectionGroupName = "ursa";

        internal const string ConfigurationSectionName = "core";
        internal const string ConfigurationSection = ConfigurationSectionGroupName + ConfigurationPathSeparator + ConfigurationSectionName;
#if CORE
        private static UrsaConfigurationSection _default;
        private const string ConfigurationPathSeparator = ":";
#else
        private const string ConfigurationPathSeparator = "/";
#endif
        private const string DefaultInstallerAssemblyNameMask = "URSA*";
        private const string InstallerAssemblyNameMaskAttribute = "installerAssemblyNameMask";
        private const string ServiceProviderTypeNameAttribute = "serviceProviderType";
        private const string ConverterProviderTypeNameAttribute = "converterProviderType";
        private const string ControllerActivatorTypeNameAttribute = "controllerActivatorType";
        private static IEnumerable<Assembly> _installerAssemblies;

#if CORE
        static UrsaConfigurationSection()
        {
            ConfigurationManager.Register<UrsaConfigurationSection>(ConfigurationSection);
        }

        /// <summary>Initializes a new instance of the <see cref="UrsaConfigurationSection" /> class.</summary>
        public UrsaConfigurationSection() : base(new ConfigurationRoot(new [] { new MemoryConfigurationProvider(new MemoryConfigurationSource()) }), ConfigurationSection)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UrsaConfigurationSection" /> class.</summary>
        /// <param name="configurationRoot">Configuration root.</param>
        /// <param name="sectionName">Configuration path.</param>
        public UrsaConfigurationSection(ConfigurationRoot configurationRoot, string sectionName) : base(configurationRoot, sectionName)
        {
        }
#endif

        /// <summary>Gets the default configuration.</summary>
        public static UrsaConfigurationSection Default
        {
            get
            {
#if CORE
                if (_default == null)
                {
                    _default = new UrsaConfigurationSection()
                        {
                            ConverterProviderTypeName = typeof(DefaultConverterProvider).FullName,
                            ControllerActivatorTypeName = typeof(DefaultControllerActivator).FullName
                        };
                    var configuration = (UrsaConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection);
                    if (configuration != null)
                    {
                        _default = configuration;
                    }
                }

                return _default;
#else
                return (UrsaConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ??
                    new UrsaConfigurationSection()
                        {
                            ConverterProviderTypeName = typeof(DefaultConverterProvider).FullName,
                            ControllerActivatorTypeName = typeof(DefaultControllerActivator).FullName
                        };
#endif
            }
        }

        /// <summary>Gets or sets the installer assembly name mask.</summary>
#if !CORE
        [ConfigurationProperty(InstallerAssemblyNameMaskAttribute)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public string InstallerAssemblyNameMask
        {
            get { return (string)this[InstallerAssemblyNameMaskAttribute]; }
            set { this[InstallerAssemblyNameMaskAttribute] = value; }
        }

        /// <summary>Gets or sets the service provider type.</summary>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public Type ServiceProviderType
        {
            get { return (ServiceProviderTypeName != null ? Type.GetType(ServiceProviderTypeName) : null); }
            set { ServiceProviderTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        /// <summary>Gets or sets the converter provider type.</summary>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public Type ConverterProviderType
        {
            get { return (ConverterProviderTypeName != null ? Type.GetType(ConverterProviderTypeName) : null); }
            set { ConverterProviderTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        /// <summary>Gets or sets the controller activator type.</summary>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public Type ControllerActivatorType
        {
            get { return (ControllerActivatorTypeName != null ? Type.GetType(ControllerActivatorTypeName) : null); }
            set { ControllerActivatorTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        /// <summary>Gets or sets a name of the service provider type.</summary>
#if !CORE
        [ConfigurationProperty(ServiceProviderTypeNameAttribute)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public string ServiceProviderTypeName
        {
            get { return (string)this[ServiceProviderTypeNameAttribute]; }
            set { this[ServiceProviderTypeNameAttribute] = value; }
        }

        /// <summary>Gets or sets a name of the converter provider type.</summary>
#if !CORE
        [ConfigurationProperty(ConverterProviderTypeNameAttribute)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public string ConverterProviderTypeName
        {
            get { return (string)this[ConverterProviderTypeNameAttribute]; }
            set { this[ConverterProviderTypeNameAttribute] = value; }
        }

        /// <summary>Gets or sets a name of the controller activator type.</summary>
#if !CORE
        [ConfigurationProperty(ControllerActivatorTypeNameAttribute)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public string ControllerActivatorTypeName
        {
            get { return (string)this[ControllerActivatorTypeNameAttribute]; }
            set { this[ControllerActivatorTypeNameAttribute] = value; }
        }

        /// <summary>Gets or sets the component provider.</summary>
        internal static IComponentProvider ComponentProvider { get; set; }

        /// <summary>Initializes the component provider configured.</summary>
        /// <returns>Implementation of the <see cref="IComponentProvider" /> pointed in the configuration with installers loaded.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Helper method used for initialization, which may proove to be difficult for testing.")]
        public static IComponentProvider InitializeComponentProvider()
        {
            if (ComponentProvider != null)
            {
                return ComponentProvider;
            }

            var configuration = Default;
            string installerAssemblyNameMask = configuration.InstallerAssemblyNameMask ?? DefaultInstallerAssemblyNameMask;
            var componentProviderCtor = GetProvider<IComponentProvider>(configuration.ServiceProviderType);
            var container = (IComponentProvider)componentProviderCtor.Invoke(null);
            var assemblies = GetInstallerAssemblies(installerAssemblyNameMask).Concat(new[] { typeof(UrsaConfigurationSection).GetTypeInfo().Assembly });
            container.Install(assemblies);
            container.InstallComponents();
            ComponentProvider = container;
            return ComponentProvider;
        }

        /// <summary>Gets the default installer assemblies.</summary>
        /// <returns>Enumeration of assemblies used by the container initialization.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Helper method used for initialization, which may proove to be difficult for testing.")]
        public static IEnumerable<Assembly> GetInstallerAssemblies()
        {
            UrsaConfigurationSection configuration = (UrsaConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ?? new UrsaConfigurationSection();
            return GetInstallerAssemblies(configuration.InstallerAssemblyNameMask ?? DefaultInstallerAssemblyNameMask);
        }

        /// <summary>Gets a constructor of a given <paramref name="type"/> and optionally of given <paramref name="parameterTypes" />.</summary>
        /// <typeparam name="T">Type required to be implemented by the given <paramref name="type" />.</typeparam>
        /// <param name="type">Type to obtain constructor for.</param>
        /// <param name="parameterTypes">Optional constructor parameters.</param>
        /// <returns><see cref="ConstructorInfo"/> matching required criteria or <b>null</b>.</returns>
        public static ConstructorInfo GetProvider<T>(Type type, params Type[] parameterTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException(String.Format("Provided '{0}' configuration is invalid.", typeof(T)));
            }

            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new ArgumentOutOfRangeException(String.Format("Provided '{1}' type is invalid and cannot be used as a '{0}'.", typeof(T), type));
            }

            var ctor = type.GetConstructor(parameterTypes ?? new Type[0]);
            if (ctor == null)
            {
                throw new InvalidOperationException(String.Format("Provided '{0}' service provided cannot be instantiated.", type));
            }

            return ctor;
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Method uses local file system, which may proove to be diffucult for testing.")]
        private static IEnumerable<Assembly> GetInstallerAssemblies(string mask)
        {
            if (_installerAssemblies != null)
            {
                return _installerAssemblies;
            }

            var loadedAssemblies = ExecutionContext.GetLoadedAssemblies(new Regex(Regex.Escape(mask).Replace("\\*", ".*").Replace("\\?", ".")));
            var loadedAssemblyFiles = loadedAssemblies.Select(assembly => System.IO.Path.GetFileNameWithoutExtension(assembly.CodeBase));
#if CORE
            Func<string, Assembly> assemblyLoader = AssemblyLoadContext.Default.LoadFromAssemblyPath;
#else
            Func<string, Assembly> assemblyLoader = Assembly.LoadFile;
#endif
            var fileAssemblies =
                from filePath in Directory.GetFiles(ExecutionContext.GetPrimaryAssemblyDirectory(), mask + ".dll")
                let fileName = System.IO.Path.GetFileNameWithoutExtension(filePath)
                where !loadedAssemblyFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase)
                select assemblyLoader(filePath);
            return _installerAssemblies = loadedAssemblies.Concat(fileAssemblies).Distinct();
        }
    }
}