using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Description.Http
{
    /// <summary>Procides an HTTP controller description building facility.</summary>
    /// <typeparam name="T"></typeparam>
    public class ControllerDescriptionBuilder<T> : IHttpControllerDescriptionBuilder<T> where T : IController
    {
        private static readonly IDictionary<string, Verb> PopularNameMappings = new Dictionary<string, Verb>()
        {
            { "Query", Verb.GET },
            { "All", Verb.GET },
            { "List", Verb.GET },
            { "Create", Verb.POST },
            { "Build", Verb.POST },
            { "Make", Verb.POST },
            { "Update", Verb.PUT },
            { "Set", Verb.PUT },
            { "Remove", Verb.DELETE },
            { "Teardown", Verb.DELETE }
        };

        private ControllerInfo<T> _description = null;
        private IDefaultParameterSourceSelector _defaultParameterSourceSelector;

        /// <summary>Initializes a new instance of the <see cref="ControllerDescriptionBuilder{T}" /> class.</summary>
        /// <param name="defaultParameterSourceSelector">Default parameter source selector.</param>
        public ControllerDescriptionBuilder(IDefaultParameterSourceSelector defaultParameterSourceSelector)
        {
            if (defaultParameterSourceSelector == null)
            {
                throw new ArgumentNullException("defaultParameterSourceSelector");
            }

            _defaultParameterSourceSelector = defaultParameterSourceSelector;
        }

        /// <inheritdoc />
        public Verb GetMethodVerb(System.Reflection.MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (!typeof(IController).IsAssignableFrom(methodInfo.DeclaringType))
            {
                throw new ArgumentOutOfRangeException("methodInfo");
            }

            return BuildDescriptor().Operations.Where(operation => operation.UnderlyingMethod == methodInfo)
                .Select(operation => ((Description.Http.OperationInfo)operation).Verb).FirstOrDefault() ?? Verb.GET;
        }

        /// <inheritdoc />
        public string GetOperationUriTemplate(MethodInfo methodInfo, out IEnumerable<ArgumentInfo> argumentMapping)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (!typeof(IController).IsAssignableFrom(methodInfo.DeclaringType))
            {
                throw new ArgumentOutOfRangeException("methodInfo");
            }

            argumentMapping = new ArgumentInfo[0];
            OperationInfo method = BuildDescriptor().Operations.Cast<OperationInfo>()
                .Where(operation => operation.UnderlyingMethod == methodInfo)
                .FirstOrDefault();
            if (method != null)
            {
                argumentMapping = method.Arguments;
                return method.UriTemplate;
            }

            return null;
        }

        /// <inheritdoc />
        ControllerInfo IControllerDescriptionBuilder.BuildDescriptor()
        {
            return BuildDescriptor();
        }

        /// <inheritdoc />
        public ControllerInfo<T> BuildDescriptor()
        {
            if (_description == null)
            {
                var route = typeof(T).GetCustomAttribute<RouteAttribute>(true) ??
                    typeof(T).GetInterfaces().Select(@interface => @interface.GetCustomAttribute<RouteAttribute>(true)).FirstOrDefault();
                Uri uri = (route != null ? route.Uri : new Uri("/" + BuildControllerName(), UriKind.Relative));
                IList<OperationInfo> operations = new List<OperationInfo>();
                var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Except(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .SelectMany(property => new MethodInfo[] { property.GetGetMethod(), property.GetSetMethod() }))
                    .Where(item => item.DeclaringType != typeof(object));
                foreach (var method in methods)
                {
                    operations.Add(BuildMethodDescriptor(method, uri));
                }

                _description = new ControllerInfo<T>(uri, operations.ToArray());
            }

            return _description;
        }

        private string BuildControllerName()
        {
            return Regex.Replace(typeof(T).Name, "Controller", String.Empty, RegexOptions.IgnoreCase).ToLower();
        }

        private OperationInfo BuildMethodDescriptor(MethodInfo method, Uri prefix)
        {
            var verb = method.GetCustomAttribute<OnVerbAttribute>(true) ??
                method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<OnVerbAttribute>()).FirstOrDefault();
            var route = method.GetCustomAttribute<RouteAttribute>(true) ??
                method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<RouteAttribute>()).FirstOrDefault();
            if ((route == null) || (verb == null))
            {
                ObtainMethodDetailsFromVerbs(method, ref route, ref verb);
            }

            if ((route == null) || (verb == null))
            {
                ObtainMethodDetailsFromPopularNames(method, ref route, ref verb);
            }

            if (route == null)
            {
                route = new RouteAttribute(method.Name.ToLower());
            }

            if (verb == null)
            {
                verb = new OnVerbAttribute(Verb.GET);
            }

            string templateRegex = Regex.Escape(route.Uri.Combine(prefix).ToString());
            Uri uri = new Uri(templateRegex, UriKind.RelativeOrAbsolute);
            string uriTemplate;
            var parameters = BuildParameterDescriptors(method, verb.Verb, ref templateRegex, out uriTemplate);
            return new Description.Http.OperationInfo(method, verb.Verb, uri, new Regex("^" + templateRegex + "$", RegexOptions.IgnoreCase), uriTemplate, parameters);
        }

        private void ObtainMethodDetailsFromVerbs(MethodInfo method, ref RouteAttribute route, ref OnVerbAttribute verb)
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

        private void ObtainMethodDetailsFromPopularNames(MethodInfo method, ref RouteAttribute route, ref OnVerbAttribute verb)
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

        //// TODO: Add support for optional uri parameters
        private ArgumentInfo[] BuildParameterDescriptors(MethodInfo method, Verb verb, ref string templateRegex, out string uriTemplate)
        {
            bool restUriTemplate = true;
            IList<ArgumentInfo> result = new List<ArgumentInfo>();
            uriTemplate = templateRegex;
            StringBuilder queryString = new StringBuilder(512);
            StringBuilder iriQueryString = new StringBuilder(512);
            int optionalParameters = 0;
            int totalParameters = 0;
            var parameters = method.GetParameters().Where(parameter => !parameter.IsOut);
            foreach (var parameter in parameters)
            {
                totalParameters++;
                bool isBodyParameter;
                string parameterUriTemplate;
                string parameterTemplateRegex;
                var source = CreateParameterTemplateRegex(method, parameter, verb, out parameterUriTemplate, out parameterTemplateRegex, out isBodyParameter);
                if ((parameterTemplateRegex != null) || (isBodyParameter))
                {
                    string variableName = (isBodyParameter ? null : parameter.Name.ToLowerCamelCase());
                    if (parameterTemplateRegex != null)
                    {
                        restUriTemplate = false;
                        if (parameterTemplateRegex[0] == '&')
                        {
                            queryString.Append("|" + parameterTemplateRegex.Substring(1).Replace("=[^&]+", String.Empty));
                            iriQueryString.Append(parameterUriTemplate);
                        }
                        else
                        {
                            templateRegex += parameterTemplateRegex;
                            uriTemplate += parameterUriTemplate;
                        }
                    }

                    result.Add(new ArgumentInfo(
                        parameter,
                        source,
                        (isBodyParameter ? null : (parameterTemplateRegex[0] == '&' ? parameterUriTemplate : uriTemplate)),
                        variableName));
                }

                if (parameter.HasDefaultValue)
                {
                    optionalParameters++;
                }
            }

            if (queryString.Length > 0)
            {
                templateRegex = templateRegex + (optionalParameters == totalParameters ? "(" : String.Empty) + "[?&](" + queryString.ToString().Substring(1) + ")=[^&]+" +
                    (optionalParameters == totalParameters ? "){0,}" : String.Empty);
                uriTemplate = uriTemplate + "?" + iriQueryString.ToString().Substring(1);
            }

            if (restUriTemplate)
            {
                uriTemplate = null;
            }

            return result.ToArray();
        }

        private ParameterSourceAttribute CreateParameterTemplateRegex(MethodInfo method, ParameterInfo parameter, Verb verb, out string parameterUriTemplate, out string parameterTemplateRegex, out bool isBodyParameter)
        {
            isBodyParameter = false;
            parameterUriTemplate = null;
            parameterTemplateRegex = null;
            var parameterSource = parameter.GetCustomAttribute<ParameterSourceAttribute>(true);
            if (parameterSource == null)
            {
                parameterSource = _defaultParameterSourceSelector.ProvideDefault(parameter, verb);
            }

            if (parameterSource is FromQueryStringAttribute)
            {
                FromQueryStringAttribute fromQueryString = (FromQueryStringAttribute)parameterSource;
                string result = (fromQueryString.UriTemplate == FromQueryStringAttribute.Default ?
                    FromQueryStringAttribute.For(parameter).UriTemplate :
                    fromQueryString.UriTemplate);
                parameterUriTemplate = result.Replace(FromUriAttribute.Value, "{?" + parameter.Name + "}");
                parameterTemplateRegex = result.Replace(FromUriAttribute.Value, "[^&]+");
            }
            else if (parameterSource is FromUriAttribute)
            {
                FromUriAttribute fromUri = (FromUriAttribute)parameterSource;
                string result = (fromUri.UriTemplate == FromUriAttribute.Default ?
                    FromUriAttribute.For(parameter).UriTemplate.ToString() :
                    fromUri.UriTemplate.ToString());
                parameterUriTemplate = result.Replace(FromUriAttribute.Value, "{?" + parameter.Name + "}");
                parameterTemplateRegex = result.Replace(FromUriAttribute.Value, "[^/?]+");
            }
            else if (parameterSource is FromBodyAttribute)
            {
                isBodyParameter = true;
            }

            return parameterSource;
        }
    }
}