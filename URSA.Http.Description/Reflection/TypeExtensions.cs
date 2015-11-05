using System;
using System.Linq;
using System.Reflection;
using URSA.Web;

namespace URSA.Reflection
{
    internal static class TypeExtensions
    {
        internal const string HydraSymbol = "hydra";
        internal const string JavascriptSymbol = "javascript";

        internal static Uri MakeUri(this Type type)
        {
            return new Uri(String.Format("{1}:{0}", type.FullName.Replace("&", String.Empty), JavascriptSymbol));
        }

        internal static Uri MakeUri(this PropertyInfo property, Type declaringTypeOverride = null)
        {
            return new Uri(String.Format("{2}:{0}.{1}", declaringTypeOverride ?? property.DeclaringType, property.Name, HydraSymbol));
        }

        internal static Type GetInterfaceImplementation(this Type implementour, Type implementation)
        {
            return (implementour.GetInterfaces().FirstOrDefault(@interface => (@interface.IsGenericType) && (implementation == @interface.GetGenericTypeDefinition()))) ??
                ((implementour.IsInterface) && (implementour.IsGenericType) && (implementation == implementour.GetGenericTypeDefinition()) ? implementour : null);
        }

        internal static bool ImplementsGeneric(this PropertyInfo implementour, Type genericType, string propertyName)
        {
            Type implementation;
            return (((implementation = implementour.DeclaringType.GetInterfaceImplementation(genericType)) != null) &&
                (implementour.Implements(implementation.GetProperty(propertyName))));
        }

        internal static bool Implements(this PropertyInfo implementour, PropertyInfo property)
        {
            return ((implementour.DeclaringType.IsInterface) && (implementour.Matches(property))) || ((!implementour.DeclaringType.IsInterface) &&
                (implementour.DeclaringType.GetInterfaceMap(property.DeclaringType).TargetMethods.Any(targetMethod => targetMethod == property.GetGetMethod())));
        }

        internal static bool Matches(this PropertyInfo leftOperand, PropertyInfo rightOperand)
        {
            return (leftOperand.Name == rightOperand.Name) && (leftOperand.PropertyType == rightOperand.PropertyType);
        }
    }
}