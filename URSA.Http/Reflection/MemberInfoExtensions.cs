using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
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

        [ExcludeFromCodeCoverage]
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

        private static void CreateParameterTemplateRegex(this ParameterInfo parameter, FromQueryStringAttribute fromQueryString, out string parameterTemplateRegex)
        {
            var parameterName = UriTemplateBuilder.VariableTemplateRegex.Match(fromQueryString.UriTemplate).Groups["ParameterName"].Value;
            parameterTemplateRegex = (parameter.HasDefaultValue) || (!parameter.ParameterType.IsValueType) ?
                String.Format("([?&]({0}=[^&]*)){{0,}}", parameterName) :
                String.Format("([?&]({0}=[^&]*)){{1,}}", parameterName);
        }

        private static void CreateParameterTemplateRegex(this ParameterInfo parameter, FromUriAttribute fromUri, out string parameterTemplateRegex)
        {
            parameterTemplateRegex = UriTemplateBuilder.VariableTemplateRegex.Replace(fromUri.UriTemplate.ToString(), "[^/?]+");
        }
    }
}