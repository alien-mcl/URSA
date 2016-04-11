using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace URSA.Configuration
{
    /// <summary>Provides useful configuration section extension methods.</summary>
    public static class ConfigurationSectionExtensions
    {
        /// <summary>Gets the constructor of the given type.</summary>
        /// <typeparam name="T">Type of the interface to be implemented.</typeparam>
        /// <param name="configSection">Config section.</param>
        /// <param name="type">Type of the provider implementing <typeparamref name="T" /> to get constructor for.</param>
        /// <param name="parameterTypes">Optional constructor parameter types.</param>
        /// <returns>Default constructor.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Helper method used mostly for validation.")]
        public static ConstructorInfo GetProvider<T>(this ConfigurationElement configSection, Type type, params Type[] parameterTypes)
        {
            if (type == null)
            {
                throw new ArgumentNullException(String.Format("Provided '{0}' configuration is invalid.", typeof(T)));
            }

            if (!typeof(T).IsAssignableFrom(type))
            {
                throw new ArgumentOutOfRangeException(String.Format("Provided '{1}' type is invalid and cannot be used as a '{0}'.", typeof(T), type));
            }

            var ctor = type.GetConstructor(parameterTypes ?? new Type[0]);
            if (ctor == null)
            {
                throw new InvalidOperationException(String.Format("Provided '{0}' service provided cannot be instantiated.", type));
            }

            return ctor;
        }
    }
}