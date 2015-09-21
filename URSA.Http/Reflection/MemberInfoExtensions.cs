using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Http.Reflection
{
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
            { "Replace", Verb.POST },
            { "Update", Verb.PUT },
            { "Remove", Verb.DELETE },
            { "Teardown", Verb.DELETE }
        };

        internal static RouteAttribute GetControllerRoute(this Type type)
        {
            while (true)
            {
                var route = type.GetCustomAttribute<RouteAttribute>(true) ??
                    type.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<RouteAttribute>(true)).FirstOrDefault();
                Type genericType = null;
                if ((!(route is DependentRouteAttribute)) || (!type.IsGenericType) ||
                    (!type.GetGenericArguments().Any(argument => (typeof(IController).IsAssignableFrom(argument)) && ((genericType = argument) != null))))
                {
                    return (route ?? new RouteAttribute("/" + type.BuildControllerName()));
                }

                type = genericType;
            }
        }

        [ExcludeFromCodeCoverage]
        private static string BuildControllerName(this Type type)
        {
            return Regex.Replace(type.Name, "Controller", String.Empty, RegexOptions.IgnoreCase).ToLower();
        }

        internal static RouteAttribute GetDefaults(this MethodInfo method, ICollection<OnVerbAttribute> verbs, out bool explicitRoute)
        {
            OnVerbAttribute verb = null;
            verbs.AddRange(method.GetCustomAttributes<OnVerbAttribute>(true) ?? method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<OnVerbAttribute>()));
            var route = method.GetCustomAttribute<RouteAttribute>(true) ??
                method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<RouteAttribute>()).FirstOrDefault();
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
            PopularNameMappings.SkipWhile(item => (methodNameWithoutVerb = Regex.Replace(methodName, "^" + item.Key, ((detectedVerb = item.Value) != null ? String.Empty : String.Empty), RegexOptions.IgnoreCase)) == methodName).ToArray();
            if ((route == null) && (methodNameWithoutVerb != null) && (methodNameWithoutVerb != methodName))
            {
                route = new RouteAttribute((methodNameWithoutVerb.Length == 0 ? "/" : methodNameWithoutVerb).ToLower());
            }

            if ((verb == null) && (methodNameWithoutVerb != null) && (methodNameWithoutVerb != methodName) && (detectedVerb != null))
            {
                verb = new OnVerbAttribute(detectedVerb);
            }
        }

        internal static bool CreateParameterTemplateRegex(this ParameterInfo parameter, ParameterSourceAttribute parameterSource, out string parameterUriTemplate, out string parameterTemplateRegex)
        {
            parameterUriTemplate = null;
            parameterTemplateRegex = null;
            var methodInfo = typeof(MemberInfoExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .FirstOrDefault(item => (item.Name == "CreateParameterTemplateRegex") && (item.GetParameters().Length > 1) && (item.GetParameters()[1].ParameterType == parameterSource.GetType()));
            if (methodInfo == null)
            {
                return false;
            }

            var arguments = new object[] { parameter, parameterSource, parameterUriTemplate, parameterTemplateRegex };
            methodInfo.Invoke(null, arguments);
            parameterUriTemplate = (string)arguments[2];
            parameterTemplateRegex = (string)arguments[3];
            return true;
        }

        private static void CreateParameterTemplateRegex(this ParameterInfo parameter, FromQueryStringAttribute fromQueryString, out string parameterUriTemplate, out string parameterTemplateRegex)
        {
            string result = (fromQueryString.UriTemplate == FromQueryStringAttribute.Default ? FromQueryStringAttribute.For(parameter).UriTemplate : fromQueryString.UriTemplate);
            parameterUriTemplate = result.Replace(FromUriAttribute.Value, "{?" + parameter.Name + "}");
            parameterTemplateRegex = result.Replace(FromUriAttribute.Value, "[^&]+");
        }

        private static void CreateParameterTemplateRegex(this ParameterInfo parameter, FromUriAttribute fromUri, out string parameterUriTemplate, out string parameterTemplateRegex)
        {
            string result = (fromUri.UriTemplate == FromUriAttribute.Default ? FromUriAttribute.For(parameter).UriTemplate.ToString() : fromUri.UriTemplate.ToString());
            parameterUriTemplate = result.Replace(FromUriAttribute.Value, "{?" + parameter.Name + "}");
            parameterTemplateRegex = result.Replace(FromUriAttribute.Value, "[^/?]+");
        }
    }
}