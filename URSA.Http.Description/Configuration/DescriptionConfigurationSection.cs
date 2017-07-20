using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
#if CORE
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
#endif
using URSA.Web.Http.Description;

namespace URSA.Configuration
{
    /// <summary>Defines possible RDF hypermedia output modes.</summary>
    public enum HypermediaModes
    {
        /// <summary>Specifies that hypermedia should be output to the transport layer, i.e. message headers.</summary>
        Transport,

        /// <summary>Specifies that hypermedia should be output to the same graph as the resulting data.</summary>
        SameGraph
    }

    /// <summary>Describes the description configuration section.</summary>
    [ExcludeFromCodeCoverage]
    public class DescriptionConfigurationSection : ConfigurationSection
    {
#if CORE
        private static DescriptionConfigurationSection _default;
#endif
        internal const string ConfigurationSectionName = "description";
        internal const string ConfigurationSection = UrsaConfigurationSection.ConfigurationSectionGroupName + ConfigurationPathSeparator + ConfigurationSectionName;
#if CORE
        private const string ConfigurationPathSeparator = ":";
#else
        private const string ConfigurationPathSeparator = "/";
#endif
        private const string TypeDescriptionBuilderTypeNameAttributeName = "typeDescriptionBuilderType";
        private const string DefaultStoreFactoryNameAttributeName = "defaultStoreFactoryName";
        private const string HypermediaModeAttributeName = "hypermediaMode";

#if CORE
        static DescriptionConfigurationSection()
        {
            ConfigurationManager.Register<DescriptionConfigurationSection>(ConfigurationSection);
        }

        /// <summary>Initializes a new instance of the <see cref="DescriptionConfigurationSection" /> class.</summary>
        public DescriptionConfigurationSection()
            : base(new ConfigurationRoot(new [] { new MemoryConfigurationProvider(new MemoryConfigurationSource()) }), ConfigurationSection)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DescriptionConfigurationSection" /> class.</summary>
        /// <param name="configurationRoot">Configuration root.</param>
        /// <param name="sectionName">Configuration path.</param>
        public DescriptionConfigurationSection(ConfigurationRoot configurationRoot, string sectionName) : base(configurationRoot, sectionName)
        {
        }
#endif

        /// <summary>Gets the default configuration.</summary>
        public static DescriptionConfigurationSection Default
        {
            get
            {
#if CORE
                if (_default == null)
                {
                    _default = new DescriptionConfigurationSection() { TypeDescriptionBuilderTypeName = typeof(HydraCompliantTypeDescriptionBuilder).FullName };
                    var configuration = (DescriptionConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection);
                    if (configuration != null)
                    {
                        _default = configuration;
                    }
                }

                return _default;
#else
                return (DescriptionConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ??
                    new DescriptionConfigurationSection() { TypeDescriptionBuilderTypeName = typeof(HydraCompliantTypeDescriptionBuilder).FullName };
#endif
            }
        }

        /// <summary>Gets or sets the hypermedia output mode.</summary>
#if !CORE
        [ConfigurationProperty(HypermediaModeAttributeName)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public HypermediaModes HypermediaMode
        {
            get
            {
#if CORE
                return (this[HypermediaModeAttributeName] == null ? HypermediaModes.Transport :
                    (HypermediaModes)Enum.Parse(typeof(HypermediaModes), this[HypermediaModeAttributeName], true));
#else
                return (HypermediaModes)this[HypermediaModeAttributeName];
#endif
            }

            set
            {
#if CORE
                this[HypermediaModeAttributeName] = value.ToString();
#else
                this[HypermediaModeAttributeName] = value;
#endif
            }
        }

        /// <summary>Gets or sets the type description builder type name.</summary>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public Type TypeDescriptionBuilderType
        {
            get { return (TypeDescriptionBuilderTypeName != null ? Type.GetType(TypeDescriptionBuilderTypeName) : null); }
            set { TypeDescriptionBuilderTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        /// <summary>Gets or sets the default name of the store factory.</summary>
#if !CORE
        [ConfigurationProperty(DefaultStoreFactoryNameAttributeName)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public string DefaultStoreFactoryName
        {
            get { return (string)this[DefaultStoreFactoryNameAttributeName]; }
            set { this[DefaultStoreFactoryNameAttributeName] = value; }
        }

        /// <summary>Gets or sets the name of a type description builder type.</summary>
#if !CORE
        [ConfigurationProperty(TypeDescriptionBuilderTypeNameAttributeName)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        public string TypeDescriptionBuilderTypeName
        {
            get { return (string)this[TypeDescriptionBuilderTypeNameAttributeName]; }
            set { this[TypeDescriptionBuilderTypeNameAttributeName] = value; }
        }
    }
}