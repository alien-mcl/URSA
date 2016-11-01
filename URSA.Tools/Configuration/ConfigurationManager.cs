using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace System.Configuration
{
    public class ConfigurationManager
    {
        private static readonly IDictionary<string, Tuple<Type, Func<IConfigurationSection, object>>> ConfigSections;
        private static readonly IConfigurationRoot Configuration;
        private static readonly IDictionary<string, IConfigurationSection> Cache;

        static ConfigurationManager()
        {
            ConfigSections = new ConcurrentDictionary<string, Tuple<Type, Func<IConfigurationSection, object>>>();
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();
            Cache = new ConcurrentDictionary<string, IConfigurationSection>();
        }

        /// <summary>Registers a given type <typeparamref name="T" /> as the one to be bound with configuration when calling <see cref="GetSection(string)" />.</summary>
        /// <typeparam name="T">Type of the section handler.</typeparam>
        /// <param name="sectionName">Section path to be bound.</param>
        /// <param name="targetPropertySelector">Optional target property to be bound.</param>
        public static void Register<T>(string sectionName, Func<T, object> targetPropertySelector = null) where T : IConfigurationSection
        {
            ConfigSections[sectionName] = new Tuple<Type, Func<IConfigurationSection, object>>(
                typeof(T),
                targetPropertySelector != null ? instance => targetPropertySelector((T)instance) : (Func<IConfigurationSection, object>)null);
        }

        /// <summary>Gets a configuration section of a given <paramref name="sectionName" />.</summary>
        /// <param name="sectionName">Path of the section to be retrieved.</param>
        /// <returns>Configuration section matching the <paramref name="sectionName" />.</returns>
        public static IConfigurationSection GetSection(string sectionName)
        {
            IConfigurationSection result;
            if (Cache.TryGetValue(sectionName, out result))
            {
                return result;
            }

            Tuple<Type, Func<IConfigurationSection, object>> registeredConfigSection;
            var configurationSection = Configuration.GetSection(sectionName);
            if ((configurationSection == null) || (!ConfigSections.TryGetValue(sectionName, out registeredConfigSection)))
            {
                return null;
            }

            if (typeof(ConfigurationSection).GetTypeInfo().IsAssignableFrom(registeredConfigSection.Item1))
            {
                result = (IConfigurationSection)registeredConfigSection.Item1.GetTypeInfo()
                    .GetConstructor(new[] { typeof(ConfigurationRoot), typeof(string) })
                    .Invoke(new object[] { Configuration, sectionName });
            }
            else
            {
                result = (IConfigurationSection)registeredConfigSection.Item1.GetTypeInfo().GetConstructor(new Type[0]).Invoke(null);
            }

            object targetInstance = result;
            if (registeredConfigSection.Item2 != null)
            {
                targetInstance = registeredConfigSection.Item2(result);
            }

            configurationSection.Bind(targetInstance);
            Cache[sectionName] = result;
            return result;
        }
    }
}
