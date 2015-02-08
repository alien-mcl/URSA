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
        private const string DefaultParameterSourceSelectorTypeNameAttribute = "defaultParameterSourceSelectorType";

        /// <summary>Gets or sets the default parameter source selector type.</summary>
        public Type DefaultParameterSourceSelectorType
        {
            get { return Type.GetType(DefaultParameterSourceSelectorTypeName); }
            set { DefaultParameterSourceSelectorTypeName = (value != null ? value.AssemblyQualifiedName : null); }
        }

        [ConfigurationProperty(DefaultParameterSourceSelectorTypeNameAttribute)]
        private string DefaultParameterSourceSelectorTypeName
        {
            get { return (string)this[DefaultParameterSourceSelectorTypeNameAttribute]; }
            set { this[DefaultParameterSourceSelectorTypeNameAttribute] = value; }
        }
    }
}