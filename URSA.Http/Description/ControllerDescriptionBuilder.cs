﻿using System;
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
                Uri uri = GetControllerRoute(typeof(T));
                IList<OperationInfo> operations = new List<OperationInfo>();
                var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Except(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .SelectMany(property => new MethodInfo[] { property.GetGetMethod(), property.GetSetMethod() }))
                    .Where(item => item.DeclaringType != typeof(object));
                foreach (var method in methods)
                {
                    operations.AddRange(BuildMethodDescriptor(method, uri));
                }

                _description = new ControllerInfo<T>(uri, operations.ToArray());
            }

            return _description;
        }

        private Uri GetControllerRoute(Type type)
        {
            var route = type.GetCustomAttribute<RouteAttribute>(true) ??
                type.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<RouteAttribute>(true)).FirstOrDefault();
            Type genericType = null;
            if ((route is DependentRouteAttribute) && (type.IsGenericType) && 
                (type.GetGenericArguments().Any(argument => (typeof(IController).IsAssignableFrom(argument)) && ((genericType = argument) != null))))
            {
                return GetControllerRoute(genericType);
            }

            return (route != null ? route.Uri : new Uri("/" + BuildControllerName(type), UriKind.Relative));
        }

        private string BuildControllerName(Type type)
        {
            return Regex.Replace(type.Name, "Controller", String.Empty, RegexOptions.IgnoreCase).ToLower();
        }

        private IEnumerable<OperationInfo> BuildMethodDescriptor(MethodInfo method, Uri prefix)
        {
            OnVerbAttribute verb = null;
            var verbs = (method.GetCustomAttributes<OnVerbAttribute>(true) ??
                method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<OnVerbAttribute>())).ToList();
            var route = method.GetCustomAttribute<RouteAttribute>(true) ??
                method.DeclaringType.GetInterfaces().Select(@interface => @interface.GetCustomAttribute<RouteAttribute>()).FirstOrDefault();
            if ((route == null) || (!verbs.Any()))
            {
                ObtainMethodDetailsFromVerbs(method, ref route, ref verb);
                if (verb != null)
                {
                    verbs.Add(verb);
                    verb = null;
                }
            }

            if ((route == null) || (!verbs.Any()))
            {
                ObtainMethodDetailsFromPopularNames(method, ref route, ref verb);
                if (verb != null)
                {
                    verbs.Add(verb);
                    verb = null;
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

            string operationTemplateRegex = Regex.Escape(route.Uri.Combine(prefix).ToString());
            Uri uri = new Uri(operationTemplateRegex, UriKind.RelativeOrAbsolute);
            IList<OperationInfo> result = new List<OperationInfo>();
            foreach (var item in verbs)
            {
                string templateRegex = operationTemplateRegex;
                string uriTemplate;
                var parameters = BuildParameterDescriptors(method, item.Verb, ref templateRegex, out uriTemplate);
                result.Add(new Description.Http.OperationInfo(method, item.Verb, uri, new Regex("^" + templateRegex + "$", RegexOptions.IgnoreCase), uriTemplate, parameters));
            }

            return result;
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

        private ArgumentInfo[] BuildParameterDescriptors(MethodInfo method, Verb verb, ref string templateRegex, out string uriTemplate)
        {
            bool restUriTemplate = true;
            IList<ArgumentInfo> result = new List<ArgumentInfo>();
            uriTemplate = templateRegex;
            StringBuilder queryString = new StringBuilder(512);
            StringBuilder iriQueryString = new StringBuilder(512);
            int optionalQueryStringParameters = 0;
            int totalQueryStringParameters = 0;
            int optionalUriParameters = 0;
            int totalUriParameters = 0;
            var parameters = method.GetParameters().Where(parameter => !parameter.IsOut);
            foreach (var parameter in parameters)
            {
                bool isBodyParameter;
                string parameterUriTemplate;
                string parameterTemplateRegex;
                var source = CreateParameterTemplateRegex(method, parameter, verb, out parameterUriTemplate, out parameterTemplateRegex, out isBodyParameter);
                Increment<FromQueryStringAttribute>(source, parameter, ref totalQueryStringParameters, ref optionalQueryStringParameters);
                Increment<FromUriAttribute>(source, parameter, ref totalUriParameters, ref optionalUriParameters);
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

                    result.Add(new ArgumentInfo(parameter, source, (isBodyParameter ? null : (parameterTemplateRegex[0] == '&' ? parameterUriTemplate : uriTemplate)), variableName));
                }
            }

            if (queryString.Length > 0)
            {
                templateRegex = templateRegex + (optionalQueryStringParameters == totalQueryStringParameters ? "(" : String.Empty) + "[?&](" + queryString.ToString().Substring(1) + ")=[^&]+" +
                    (optionalQueryStringParameters == totalQueryStringParameters ? "){0,}" : String.Empty);
                uriTemplate = uriTemplate + "?" + iriQueryString.ToString().Substring(1);
            }

            if (restUriTemplate)
            {
                uriTemplate = null;
            }

            return result.ToArray();
        }

        private void Increment<S>(ParameterSourceAttribute parameterSource, ParameterInfo parameter, ref int total, ref int optional)
        {
            if (typeof(S).IsAssignableFrom(parameterSource.GetType()))
            {
                total++;
                if (parameter.HasDefaultValue)
                {
                    optional++;
                }
            }
        }

        private ParameterSourceAttribute CreateParameterTemplateRegex(MethodInfo method, ParameterInfo parameter, Verb verb, out string parameterUriTemplate, out string parameterTemplateRegex, out bool isBodyParameter)
        {
            isBodyParameter = false;
            parameterUriTemplate = null;
            parameterTemplateRegex = null;
            var parameterSource = parameter.GetCustomAttribute<ParameterSourceAttribute>(true) ?? _defaultParameterSourceSelector.ProvideDefault(parameter, verb);
            var methodInfo = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(item => (item.Name == "CreateParameterTemplateRegex") && (item.GetParameters().Length > 0) && (item.GetParameters()[0].ParameterType == parameterSource.GetType()))
                .FirstOrDefault();
            if (methodInfo != null)
            {
                var arguments = new object[] { parameterSource, method, parameter, verb, parameterUriTemplate, parameterTemplateRegex };
                methodInfo.Invoke(this, arguments);
                parameterUriTemplate = (string)arguments[4];
                parameterTemplateRegex = (string)arguments[5];

            }
            else if (parameterSource is FromBodyAttribute)
            {
                isBodyParameter = true;
            }

            return parameterSource;
        }

        private void CreateParameterTemplateRegex(FromQueryStringAttribute fromQueryString, MethodInfo method, ParameterInfo parameter, Verb verb, out string parameterUriTemplate, out string parameterTemplateRegex)
        {
            string result = (fromQueryString.UriTemplate == FromQueryStringAttribute.Default ? FromQueryStringAttribute.For(parameter).UriTemplate : fromQueryString.UriTemplate);
            parameterUriTemplate = result.Replace(FromUriAttribute.Value, "{?" + parameter.Name + "}");
            parameterTemplateRegex = result.Replace(FromUriAttribute.Value, "[^&]+");
        }

        private void CreateParameterTemplateRegex(FromUriAttribute fromUri, MethodInfo method, ParameterInfo parameter, Verb verb, out string parameterUriTemplate, out string parameterTemplateRegex)
        {
            string result = (fromUri.UriTemplate == FromUriAttribute.Default ? FromUriAttribute.For(parameter).UriTemplate.ToString() : fromUri.UriTemplate.ToString());
            parameterUriTemplate = result.Replace(FromUriAttribute.Value, "{?" + parameter.Name + "}");
            parameterTemplateRegex = (parameter.HasDefaultValue ? "(" : String.Empty) + result.Replace(FromUriAttribute.Value, String.Format("[^/?]+{0}", (parameter.HasDefaultValue ? ")?" : String.Empty)));
        }
    }
}