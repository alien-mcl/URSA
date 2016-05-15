using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RomanticWeb.Entities;
using URSA.Web;
using URSA.Web.Http.Description.Reflection;

namespace URSA.Reflection
{
    internal static class TypeExtensions
    {
        internal const string HydraSymbol = "hydra";
        internal const string JavascriptSymbol = "javascript";

        internal static Uri MakeUri(this Type type)
        {
            return new Uri(String.Format("{1}:{0}", type.MakeTypeName(), JavascriptSymbol));
        }

        internal static string MakeTypeName(this Type type, bool includeNamespace = true, bool keepSyntax = false)
        {
            if (type.IsArray)
            {
                return type.GetElementType().MakeTypeName(includeNamespace, keepSyntax) + "[]";
            }

            if (!type.IsGenericType)
            {
                return (includeNamespace ? type.FullName : type.Name).Replace("&", String.Empty);
            }

            var typeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            return String.Format(
                (keepSyntax ? "{0}.{1}<{2}>" : "{0}.{1}Of{2}"),
                type.Namespace,
                (type.IsInterface ? typeName.TrimStart('I') : typeName),
                String.Join((keepSyntax ? "," : "And"), type.GetGenericArguments().Select(genericType => genericType.MakeTypeName(keepSyntax && includeNamespace, keepSyntax))));
        }

        internal static Uri MakeUri(this PropertyInfo property, Type declaringTypeOverride = null)
        {
            return new Uri(String.Format("urn:{2}:{0}.{1}", declaringTypeOverride ?? property.DeclaringType, property.Name, HydraSymbol));
        }

        internal static Type GetInterfaceImplementation(this Type implementour, Type implementation, Func<Type, bool> withPredicate = null)
        {
            return (implementour.GetInterfaces().FirstOrDefault(@interface => @interface.IsGenericBaseOf(implementation, withPredicate))) ??
                ((implementour.IsInterface) && (implementour.IsGenericBaseOf(implementation, withPredicate)) ? implementour : null);
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

        internal static IEnumerable<PropertyInfo> GetProperties(this Type type, params Type[] exceptInterface)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Union(type.GetInterfaces().Except(exceptInterface).SelectMany(@interface => @interface.GetProperties()))
                .Distinct(PropertyEqualityComparer.Default);
        }

        internal static PropertyInfo MatchesPropertyOf(this MethodInfo method, Type targetType, params string[] exceptNamedProperty)
        {
            return (from parameter in method.GetParameters()
                    from property in targetType.GetProperties(typeof(IEntity))
                    where ((parameter.ParameterType == property.PropertyType) || (parameter.ParameterType.IsAssignableFrom(property.PropertyType))) && 
                        (StringComparer.OrdinalIgnoreCase.Equals(parameter.Name, property.Name.ToLower())) &&
                        (!exceptNamedProperty.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    select property).FirstOrDefault();
        }

        private static bool IsGenericBaseOf(this Type @interface, Type implementation, Func<Type, bool> withPredicate = null)
        {
            return (@interface.IsGenericType) && (@interface.GetGenericTypeDefinition() == implementation) &&
                ((withPredicate == null) || (withPredicate(@interface)));
        }
    }
}