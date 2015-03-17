using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Configuration
{
    /// <summary>Describes the base URSA configuration section.</summary>
    [ExcludeFromCodeCoverage]
    public class HttpConfigurationSection : ConfigurationSection
    {
        internal const string ConfigurationSectionName = "http";
        internal const string ConfigurationSection = UrsaConfigurationSection.ConfigurationSectionGroupName + "/" + ConfigurationSectionName;
        private const string DefaultValueRelationSelectorTypeNameAttribute = "defaultValueRelationSelectorType";

        /// <summary>Gets or sets the default parameter source selector type.</summary>
        public Type DefaultValueRelationSelectorType
        {
            get { return Type.GetType(DefaultValueRelationSelectorTypeName); }
            set { DefaultValueRelationSelectorTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        [ConfigurationProperty(DefaultValueRelationSelectorTypeNameAttribute)]
        private string DefaultValueRelationSelectorTypeName
        {
            get { return (string)this[DefaultValueRelationSelectorTypeNameAttribute]; }
            set { this[DefaultValueRelationSelectorTypeNameAttribute] = value; }
        }
    }
}