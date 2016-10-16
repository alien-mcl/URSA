using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using URSA.Web.Description.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Reflection
{
    /// <summary>Provides useful reflection member extensions.</summary>
    public static class MemberInfoExtensions
    {
        /// <summary>Exposes popular names - HTTP verb mappings.</summary>
        public static readonly IDictionary<string, Verb> PopularNameMappings = new Dictionary<string, Verb>()
        {
            { "Query", Verb.GET },
            { "All", Verb.GET },
            { "List", Verb.GET },
            { "Create", Verb.POST },
            { "Build", Verb.POST },
            { "Make", Verb.POST },
            { "Set", Verb.POST },
            { "Do", Verb.POST },
            { "Replace", Verb.POST },
            { "Update", Verb.PUT },
            { "Remove", Verb.DELETE },
            { "Teardown", Verb.DELETE }
        };

        internal static bool SetPropertyValue(this object instance, PropertyInfo property, object value)
        {
            if (value == null)
            {
                return false;
            }

            var propertyTypeInfo = property.PropertyType.GetTypeInfo();
            if (!propertyTypeInfo.GetItemType().ConvertValue(ref value))
            {
                return false;
            }

            if (propertyTypeInfo.IsEnumerable())
            {
                return instance.SetPropertyValues(property, value);
            }

            property.SetValue(instance, value);
            return true;
        }

        internal static bool SetPropertyValues(this object instance, PropertyInfo property, object value)
        {
            var container = (IEnumerable)property.GetValue(instance);
            var itemType = property.PropertyType.GetTypeInfo().GetItemType();
            if (property.PropertyType.IsArray)
            {
                return property.SetPropertyArrayValue(instance, container, value);
            }

            if ((container != null) && (container.GetType().GetTypeInfo().GetImplementationOfAny(typeof(ICollection<>), typeof(ICollection)) != null))
            {
                container.GetType().GetMethod("Add", BindingFlags.Instance | BindingFlags.Public).Invoke(container, new[] { value });
                return true;
            }

            if (!property.CanWrite)
            {
                return false;
            }

            var list = (IList)typeof(List<>).MakeGenericType(itemType).GetConstructor(new Type[0]).Invoke(null);
            property.SetValue(instance, list);
            container.ForEach(item => list.Add(item));
            list.Add(value);
            return true;
        }

        internal static RouteAttribute GetControllerRoute(this Type type)
        {
            while (true)
            {
                var typeInfo = type.GetTypeInfo();
                var route = typeInfo.GetCustomAttribute<RouteAttribute>(true) ??
                    type.GetInterfaces().Select(@interface => @interface.GetTypeInfo().GetCustomAttribute<RouteAttribute>(true)).FirstOrDefault();
                Type genericType = null;
                if ((!(route is DependentRouteAttribute)) || (!typeInfo.IsGenericType) ||
                    (!type.GetGenericArguments().Any(argument => (typeof(IController).IsAssignableFrom(argument)) && ((genericType = argument) != null))))
                {
                    return (route ?? new RouteAttribute("/" + type.BuildControllerName()));
                }

                type = genericType;
            }
        }

        internal static RouteAttribute GetDefaults(this MethodInfo method, ICollection<OnVerbAttribute> verbs, out bool explicitRoute)
        {
            OnVerbAttribute verb = null;
            verbs.AddRange(method.GetCustomAttributes<OnVerbAttribute>(true) ??
                method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetTypeInfo().GetCustomAttribute<OnVerbAttribute>()));
            var route = method.GetCustomAttribute<RouteAttribute>(true) ??
                method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetTypeInfo().GetCustomAttribute<RouteAttribute>()).FirstOrDefault();
            explicitRoute = (route != null);
            if ((route == null) || (!verbs.Any()))
            {
                method.ObtainMethodDetailsFromVerbs(ref route, ref verb);
                if (verb != null)
                {
                    verbs.Add(verb);
                    verb = null;
                }
            }

            if ((route == null) || (!verbs.Any()))
            {
                method.ObtainMethodDetailsFromPopularNames(ref route, ref verb);
                if (verb != null)
                {
                    verbs.Add(verb);
                }
            }

            if (route == null)
            {
                route = new RouteAttribute(method.Name.ToLower());
            }

            if (!verbs.Any())
            {
                verbs.Add(new OnVerbAttribute(Verb.GET));
            }

            return route;
        }

        internal static bool CreateParameterTemplateRegex(this ParameterInfo parameter, ParameterSourceAttribute parameterSource, out string parameterTemplateRegex)
        {
            parameterTemplateRegex = null;
            var methodInfo = typeof(MemberInfoExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(item => (item.Name == "CreateParameterTemplateRegex") && (item.GetParameters().Length > 1) && (item.GetParameters()[1].ParameterType == parameterSource.GetType()));
            if (methodInfo == null)
            {
                return false;
            }

            var arguments = new object[] { parameter, parameterSource, null };
            methodInfo.Invoke(null, arguments);
            parameterTemplateRegex = (string)arguments[2];
            return true;
        }

        internal static Type UnwrapIfTask(this Type type)
        {
            return ((type.GetTypeInfo().IsGenericType) && (typeof(Task<>).IsAssignableFrom(type.GetGenericTypeDefinition())) ? type.GetGenericArguments()[0] : type);
        }

        internal static bool IsIdentity(this Type type)
        {
            return ((typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type) ||
                (typeof(Int32).MakeByRefType() == type) || (typeof(UInt32).MakeByRefType() == type) || (typeof(Int64).MakeByRefType() == type) || (typeof(UInt64).MakeByRefType() == type));
        }

        internal static bool IsNumber(this Type type)
        {
            return (typeof(SByte) == type) || (typeof(Byte) == type) || (typeof(Int16) == type) || (typeof(UInt16) == type) ||
                (typeof(Int32) == type) || (typeof(UInt32) == type) || (typeof(Int64) == type) || (typeof(UInt64) == type) ||
                (typeof(Single) == type) || (typeof(Double) == type) || (typeof(Decimal) == type);
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        private static string BuildControllerName(this Type type)
        {
            return Regex.Replace(type.Name, "Controller", String.Empty, RegexOptions.IgnoreCase).ToLower();
        }

        private static void ObtainMethodDetailsFromVerbs(this MethodInfo method, ref RouteAttribute route, ref OnVerbAttribute verb)
        {
            string methodNameWithoutVerb = null;
            Verb detectedVerb = null;
            string methodName = method.Name;
            Verb.Verbs.SkipWhile(item => (methodNameWithoutVerb = Regex.Replace(methodName, "^" + (detectedVerb = item).ToString(), String.Empty, RegexOptions.IgnoreCase)) == methodName).ToArray();
            if ((route == null) && (methodNameWithoutVerb != null) && (methodNameWithoutVerb != methodName))
            {
                route = new RouteAttribute((methodNameWithoutVerb.Length == 0 ? "/" : methodNameWithoutVerb).ToLower());
            }

            if ((verb == null) && (methodNameWithoutVerb != null) && (methodNameWithoutVerb != methodName) && (detectedVerb != null))
            {
                verb = new OnVerbAttribute(detectedVerb);
            }
        }

        private static void ObtainMethodDetailsFromPopularNames(this MethodInfo method, ref RouteAttribute route, ref OnVerbAttribute verb)
        {
            string methodNameWithoutVerb = null;
            Verb detectedVerb = null;
            string methodName = method.Name;
            PopularNameMappings.SkipWhile(item => (methodNameWithoutVerb = Regex.Replace(methodName, "^" + item.Key + "((?=[A-Z0-9])|$)", ((detectedVerb = item.Value) != null ? String.Empty : String.Empty), RegexOptions.IgnoreCase)) == methodName).ToArray();
            if ((route == null) && (methodNameWithoutVerb != null) && (methodNameWithoutVerb != methodName))
            {
                route = new RouteAttribute((methodNameWithoutVerb.Length == 0 ? "/" : methodNameWithoutVerb).ToLower());
            }

            if ((verb == null) && (methodNameWithoutVerb != null) && (methodNameWithoutVerb != methodName) && (detectedVerb != null))
            {
                verb = new OnVerbAttribute(detectedVerb);
            }
        }

        private static void CreateParameterTemplateRegex(this ParameterInfo parameter, FromQueryStringAttribute fromQueryString, out string parameterTemplateRegex)
        {
            var parameterName = UriTemplateBuilder.VariableTemplateRegex.Match(fromQueryString.UrlTemplate).Groups["ParameterName"].Value;
            parameterTemplateRegex = (parameter.HasDefaultValue) || (!parameter.ParameterType.GetTypeInfo().IsValueType) ?
                String.Format("([?&]({0}=[^&]*)){{0,}}", parameterName) :
                String.Format("([?&]({0}=[^&]*)){{1,}}", parameterName);
        }

        private static void CreateParameterTemplateRegex(this ParameterInfo parameter, FromUrlAttribute fromUrl, out string parameterTemplateRegex)
        {
            parameterTemplateRegex = UriTemplateBuilder.VariableTemplateRegex.Replace(fromUrl.UrlTemplate.ToString(), "[^/?]+");
        }

        private static bool SetPropertyArrayValue(this PropertyInfo property, object instance, IEnumerable container, object value)
        {
            var itemType = property.PropertyType.GetTypeInfo().GetItemType();
            var collection = (ICollection)container;
            if ((container == null) && (!property.CanWrite))
            {
                return false;
            }

            var array = Array.CreateInstance(itemType, (collection != null ? collection.Count + 1 : 1));
            property.SetValue(instance, array);
            if (collection != null)
            {
                collection.CopyTo(array, 0);
            }

            new[] { value }.CopyTo(array, array.Length - 1);
            return true;
        }

        private static bool ConvertValue(this Type type, ref object value)
        {
            if (type.IsInstanceOfType(value))
            {
                return true;
            }

            if (value is string)
            {
                if (((string)value).Length == 0)
                {
                    return false;
                }

                var typeConverter = TypeDescriptor.GetConverter(type);
                value = typeConverter.ConvertFromInvariantString((string)value);
                return true;
            }

            try
            {
                value = Convert.ChangeType(value, type);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}