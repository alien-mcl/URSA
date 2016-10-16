using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
#if CORE
using Microsoft.Extensions.Configuration;
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
        internal const string ConfigurationSectionName = "description";
        internal const string ConfigurationSection = UrsaConfigurationSection.ConfigurationSectionGroupName + "/" + ConfigurationSectionName;
        private const string TypeDescriptionBuilderTypeNameAttributeName = "typeDescriptionBuilderType";
        private const string DefaultStoreFactoryNameAttributeName = "defaultStoreFactoryName";
        private const string HypermediaModeAttributeName = "hypermediaMode";

#if CORE
        static DescriptionConfigurationSection()
        {
            ConfigurationManager.Register<DescriptionConfigurationSection>(ConfigurationSection);
        }

        /// <summary>Initializes a new instance of the <see cref="DescriptionConfigurationSection" /> class.</summary>
        public DescriptionConfigurationSection() : base(new ConfigurationRoot(new IConfigurationProvider[0]), ConfigurationSection)
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
                return (DescriptionConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ??
                    new DescriptionConfigurationSection() { TypeDescriptionBuilderTypeName = typeof(HydraCompliantTypeDescriptionBuilder).FullName };
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
                return (HypermediaModes)Enum.Parse(typeof(HypermediaModes), this[HypermediaModeAttributeName], true);
#else
                return (HypermediaModes)this[HypermediaModeAttributeName];
#endif
            }

            set
            {
#if CORE
                this[HypermediaModeAttributeName] = (value != null ? value.ToString() : null);
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
            get { return Type.GetType(TypeDescriptionBuilderTypeName); }
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

#if !CORE
        [ConfigurationProperty(TypeDescriptionBuilderTypeNameAttributeName)]
#endif
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
        private string TypeDescriptionBuilderTypeName
        {
            get { return (string)this[TypeDescriptionBuilderTypeNameAttributeName]; }
            set { this[TypeDescriptionBuilderTypeNameAttributeName] = value; }
        }
    }
}