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

        static ConfigurationManager()
        {
            ConfigSections = new ConcurrentDictionary<string, Tuple<Type, Func<IConfigurationSection, object>>>();
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();
        }

        /// <summary>Registers a given type <typeparamref name="T" /> as the one to be bound with configuration when calling <see cref="GetSection(string)" />.</summary>
        /// <typeparam name="T">Type of the section handler.</typeparam>
        /// <param name="sectionName">Section path to be bound.</param>
        /// <param name="targetPropertySelector">Optional target property to be bound.</param>
        public static void Register<T>(string sectionName, Func<T, object> targetPropertySelector = null) where T : IConfigurationSection
        {
            ConfigSections[sectionName] = new Tuple<Type, Func<IConfigurationSection, object>>(typeof(T), instance => targetPropertySelector((T)instance));
        }

        /// <summary>Gets a configuration section of a given <paramref name="sectionName" />.</summary>
        /// <param name="sectionName">Path of the section to be retrieved.</param>
        /// <returns>Configuration section matching the <paramref name="sectionName" />.</returns>
        public static IConfigurationSection GetSection(string sectionName)
        {
            Tuple<Type, Func<IConfigurationSection, object>> registeredConfigSection;
            if (ConfigSections.TryGetValue(sectionName, out registeredConfigSection))
            {
                IConfigurationSection instance;
                if (typeof(ConfigurationSection).GetTypeInfo().IsAssignableFrom(registeredConfigSection.Item1))
                {
                    instance = (IConfigurationSection)registeredConfigSection.Item1.GetTypeInfo()
                        .GetConstructor(new[] { typeof(ConfigurationRoot), typeof(string) })
                        .Invoke(new object[] { Configuration, sectionName });
                }
                else
                {
                    instance = (IConfigurationSection)registeredConfigSection.Item1.GetTypeInfo().GetConstructor(new Type[0]).Invoke(null);
                }

                object targetInstance = instance;
                if (registeredConfigSection.Item2 != null)
                {
                    targetInstance = registeredConfigSection.Item2(instance);
                }

                Configuration.GetSection(sectionName).Bind(targetInstance);
                return instance;
            }

            return Configuration.GetSection(sectionName);
        }
    }
}
