using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
#if CORE
using Microsoft.Extensions.Configuration;
#else
using System.Reflection;
using System.Xml;
#endif

namespace URSA.Configuration
{
    /// <summary>Provides a programmatic access to URL parsers configuration.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Wrapper without testable logic.")]
    public class UrlParserConfigurationSection : ConfigurationSection
    {
#if CORE
        private const string ConfigurationPathSeparator = ":";
#else
        private const string ConfigurationPathSeparator = "/";
#endif
        private const string ConfigurationSectionName = "urlParsers";
        private const string ConfigurationSection = UrsaConfigurationSection.ConfigurationSectionGroupName + ConfigurationPathSeparator + ConfigurationSectionName;

        private readonly IDictionary<Type, IEnumerable<string>> _parsers = new ConcurrentDictionary<Type, IEnumerable<string>>();

#if CORE
        static UrlParserConfigurationSection()
        {
            ConfigurationManager.Register<UrlParserConfigurationSection>(ConfigurationSection, instance => instance._parsers);
        }

        /// <summary>Gets the default configuration</summary>
        public static UrlParserConfigurationSection Default
        {
            get
            {
                return (UrlParserConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ?? new UrlParserConfigurationSection();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="UrlParserConfigurationSection" /> class.</summary>
        public UrlParserConfigurationSection() : base(new ConfigurationRoot(new IConfigurationProvider[0]), ConfigurationSectionName)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UrlParserConfigurationSection" /> class.</summary>
        /// <param name="configurationRoot">Configuration root.</param>
        /// <param name="sectionName">Configuration path.</param>
        public UrlParserConfigurationSection(ConfigurationRoot configurationRoot, string sectionName) : base(configurationRoot, sectionName)
        {
        }
#else
        /// <summary>Gets the default configuration</summary>
        public static UrlParserConfigurationSection Default
        {
            get
            {
                return (UrlParserConfigurationSection)ConfigurationManager.GetSection(ConfigurationSection) ?? new UrlParserConfigurationSection();
            }
        }
#endif

        /// <summary>Gets the registered parsers and their schemes.</summary>
        public IEnumerable<KeyValuePair<Type, IEnumerable<string>>> Parsers { get { return _parsers; } }

#if !CORE
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
                            if ((type != null) && (typeof(UrlParser).GetTypeInfo().IsAssignableFrom(type)) && (!type.GetTypeInfo().IsAbstract))
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
#endif
    }
}