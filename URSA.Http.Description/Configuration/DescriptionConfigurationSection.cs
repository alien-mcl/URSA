using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        public HypermediaModes HypermediaMode
        {
            get
            {
                return (!Properties.Cast<ConfigurationProperty>().Any(property => property.Name == HypermediaModeAttributeName) ?
                    HypermediaModes.Transport : (HypermediaModes)this[HypermediaModeAttributeName]);
            }

            set
            {
                this[HypermediaModeAttributeName] = value;
            }
        }

        /// <summary>Gets or sets the type description builder type name.</summary>
        public Type TypeDescriptionBuilderType
        {
            get { return Type.GetType(TypeDescriptionBuilderTypeName); }
            set { TypeDescriptionBuilderTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        /// <summary>Gets or sets the default name of the store factory.</summary>
        [ConfigurationProperty(DefaultStoreFactoryNameAttributeName)]
        public string DefaultStoreFactoryName
        {
            get { return (string)this[DefaultStoreFactoryNameAttributeName]; }
            set { this[DefaultStoreFactoryNameAttributeName] = value; }
        }

        [ConfigurationProperty(TypeDescriptionBuilderTypeNameAttributeName)]
        private string TypeDescriptionBuilderTypeName
        {
            get { return (string)this[TypeDescriptionBuilderTypeNameAttributeName]; }
            set { this[TypeDescriptionBuilderTypeNameAttributeName] = value; }
        }
    }
}