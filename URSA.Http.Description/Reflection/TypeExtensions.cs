using System;
using System.Reflection;

namespace URSA.Reflection
{
    internal static class TypeExtensions
    {
        internal const string HydraSymbol = "hydra";
        internal const string DotNetSymbol = "net";
        internal const string DotNetEnumerableSymbol = "net-enumerable";
        internal const string DotNetListSymbol = "net-list";

        internal static Uri MakeUri(this Type type)
        {
            string subScheme = DotNetSymbol;
            if (type.IsList())
            {
                subScheme = DotNetListSymbol;
                type = type.FindItemType();
            }
            else if (System.Reflection.TypeExtensions.IsEnumerable(type))
            {
                subScheme = DotNetEnumerableSymbol;
            }

            return new Uri(String.Format("urn:{1}:{0}", type.FullName.Replace("&", String.Empty), subScheme));
        }

        internal static Uri MakeUri(this PropertyInfo property)
        {
            return new Uri(String.Format("urn:{2}:{0}.{1}", property.DeclaringType, property.Name, HydraSymbol));
        }
    }
}