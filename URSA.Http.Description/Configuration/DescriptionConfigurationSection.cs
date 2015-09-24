using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http.Description;

namespace URSA.Configuration
{
    /// <summary>Describes the description configuration section.</summary>
    [ExcludeFromCodeCoverage]
    public class DescriptionConfigurationSection : ConfigurationSection
    {
        internal const string ConfigurationSectionName = "description";
        internal const string ConfigurationSection = UrsaConfigurationSection.ConfigurationSectionGroupName + "/" + ConfigurationSectionName;
        private const string TypeDescriptionBuilderTypeNameAttributeName = "typeDescriptionBuilderType";

        /// <summary>Gets the default configuration.</summary>
        public static DescriptionConfigurationSection Default
        {
            get
            {
                return (DescriptionConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ??
                    new DescriptionConfigurationSection() { TypeDescriptionBuilderTypeName = typeof(HydraCompliantTypeDescriptionBuilder).FullName };
            }
        }

        /// <summary>Gets or sets the type description builder type name.</summary>
        public Type TypeDescriptionBuilderType
        {
            get { return Type.GetType(TypeDescriptionBuilderTypeName); }
            set { TypeDescriptionBuilderTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        [ConfigurationProperty(TypeDescriptionBuilderTypeNameAttributeName)]
        private string TypeDescriptionBuilderTypeName
        {
            get { return (string)this[TypeDescriptionBuilderTypeNameAttributeName]; }
            set { this[TypeDescriptionBuilderTypeNameAttributeName] = value; }
        }
    }
}