using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace URSA.Configuration
{
    /// <summary>Provides a programmatic access to URL parsers configuration.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
    public class UrlParserConfigurationSection : ConfigurationSection
    {
        private const string ConfigurationSectionName = "urlParsers";
        private const string ConfigurationSection = UrsaConfigurationSection.ConfigurationSectionGroupName + "/" + ConfigurationSectionName;

        private readonly IDictionary<Type, IEnumerable<string>> _parsers = new ConcurrentDictionary<Type, IEnumerable<string>>();

        /// <summary>Gets the default configuration</summary>
        public static UrlParserConfigurationSection Default
        {
            get
            {
                return (UrlParserConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ?? new UrlParserConfigurationSection();
            }
        }

        /// <summary>Gets the registered parsers and their schemes.</summary>
        public IEnumerable<KeyValuePair<Type, IEnumerable<string>>> Parsers { get { return _parsers; } }

        /// <inheritdoc />
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            if ((reader.NodeType != XmlNodeType.Element) || (reader.Name != ConfigurationSectionName))
            {
                base.DeserializeElement(reader, serializeCollectionKey);
                return;
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.Name == ConfigurationSectionName)
                        {
                            return;
                        }

                        break;
                    case XmlNodeType.Element:
                        if (reader.Name != "add")
                        {
                            break;
                        }

                        string scheme = reader.GetAttribute("scheme");
                        string typeName = reader.GetAttribute("type");
                        if (!String.IsNullOrEmpty(typeName))
                        {
                            var type = Type.GetType(typeName);
                            if ((type != null) && (typeof(UrlParser).IsAssignableFrom(type)) && (!type.IsAbstract))
                            {
                                IEnumerable<string> registeredSchemes;
                                if (!_parsers.TryGetValue(type, out registeredSchemes))
                                {
                                    _parsers[type] = registeredSchemes = new HashSet<string>();
                                }

                                if (!String.IsNullOrEmpty(scheme))
                                {
                                    ((HashSet<string>)registeredSchemes).Add(scheme);
                                }
                            }
                        }

                        break;
                }
            }
        }
    }
}