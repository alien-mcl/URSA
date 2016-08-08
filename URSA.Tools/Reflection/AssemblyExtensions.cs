using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Reflection
{
    /// <summary>Provides useful <see cref="Assembly" /> related extension methods.</summary>
    public static class AssemblyExtensions
    {
        private static readonly IDictionary<string, object> EnumCache = new ConcurrentDictionary<string, object>();

        /// <summary>Searches types in assemblies for an enumeration value that has string representation of the given <paramref name="value" />.</summary>
        /// <remarks>This method uses <see cref="StringComparer.OrdinalIgnoreCase" /> to compare string representations.</remarks>
        /// <param name="assemblies">Assemblies to be searched through.</param>
        /// <param name="value">Value to be searched for.</param>
        /// <returns>Instance of the enumeration value if matched; otherwise <b>null</b>.</returns>
        [ExcludeFromCodeCoverage]
        public static object FindEnumValue(this IEnumerable<Assembly> assemblies, string value)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.Length == 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            object result;
            if (EnumCache.TryGetValue(value, out result))
            {
                return result;
            }

            foreach (var assembly in assemblies)
            {
                try
                {
                    result = (from type in assembly.GetTypes()
                              let typeInfo = type.GetTypeInfo()
                              where (typeInfo.IsEnum) && (!typeInfo.IsGenericType)
                              from enumValue in Enum.GetValues(type).Cast<object>()
                              where StringComparer.OrdinalIgnoreCase.Equals(enumValue.ToString(), value)
                              select enumValue).FirstOrDefault();
                    if (result == null)
                    {
                        continue;
                    }

                    EnumCache[value] = result;
                    break;
                }
                catch
                {
                }
            }

            return result;
        }
    }
}