using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
#if CORE
using Microsoft.Extensions.Configuration;
#endif

namespace URSA.Configuration
{
    /// <summary>Describes the base URSA configuration section.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
    public class HttpConfigurationSection : ConfigurationSection
    {
        /// <summary>Exposes a default name of the URSA HTTP configuration section.</summary>
        public const string ConfigurationSection = UrsaConfigurationSection.ConfigurationSectionGroupName + ConfigurationPathSeparator + ConfigurationSectionName;

        internal const string ConfigurationSectionName = "http";
        private const string DefaultValueRelationSelectorTypeNameAttribute = "defaultValueRelationSelectorType";
#if CORE
        private const string ConfigurationPathSeparator = ":";
#else
        private const string ConfigurationPathSeparator = "/";
#endif

#if CORE
        static HttpConfigurationSection()
        {
            ConfigurationManager.Register<HttpConfigurationSection>(ConfigurationSection);
        }

        /// <summary>Initializes a new instance of the <see cref="HttpConfigurationSection" /> class.</summary>
        public HttpConfigurationSection() : base(new ConfigurationRoot(new IConfigurationProvider[0]), ConfigurationSectionName)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="HttpConfigurationSection" /> class.</summary>
        /// <param name="configurationRoot">Configuration root.</param>
        /// <param name="sectionName">Configuration path.</param>
        public HttpConfigurationSection(ConfigurationRoot configurationRoot, string sectionName) : base(configurationRoot, sectionName)
        {
        }
#endif

        /// <summary>Gets or sets the default parameter source selector type.</summary>
        public Type DefaultValueRelationSelectorType
        {
            get { return Type.GetType(DefaultValueRelationSelectorTypeName); }
            set { DefaultValueRelationSelectorTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

#if !CORE
        [ConfigurationProperty(DefaultValueRelationSelectorTypeNameAttribute)]
#endif
        private string DefaultValueRelationSelectorTypeName
        {
            get { return (string)this[DefaultValueRelationSelectorTypeNameAttribute]; }
            set { this[DefaultValueRelationSelectorTypeNameAttribute] = value; }
        }
    }
}