using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using URSA.Security;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Description.Http
{
    /// <summary>Provides an HTTP controller description building facility.</summary>
    /// <typeparam name="T">Type of the controller being described.</typeparam>
    public class ControllerDescriptionBuilder<T> : IHttpControllerDescriptionBuilder<T> where T : IController
    {
        private readonly Lazy<ControllerInfo<T>> _description;
        private readonly IDefaultValueRelationSelector _defaultValueRelationSelector;

        /// <summary>Initializes a new instance of the <see cref="ControllerDescriptionBuilder{T}" /> class.</summary>
        /// <param name="defaultValueRelationSelector">Default parameter source selector.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public ControllerDescriptionBuilder(IDefaultValueRelationSelector defaultValueRelationSelector)
        {
            if (defaultValueRelationSelector == null)
            {
                throw new ArgumentNullException("defaultValueRelationSelector");
            }

            _defaultValueRelationSelector = defaultValueRelationSelector;
            _description = new Lazy<ControllerInfo<T>>(BuildDescriptorInternal, LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <inheritdoc />
        public Verb GetMethodVerb(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (!typeof(IController).IsAssignableFrom(methodInfo.DeclaringType))
            {
                throw new ArgumentOutOfRangeException("methodInfo");
            }

            return _description.Value.Operations.Where(operation => operation.UnderlyingMethod == methodInfo)
                .Select(operation => ((OperationInfo<Verb>)operation).ProtocolSpecificCommand).FirstOrDefault() ?? Verb.GET;
        }

        /// <inheritdoc />
        public string GetOperationUrlTemplate(MethodInfo methodInfo, out IEnumerable<ArgumentInfo> argumentMapping)
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
            OperationInfo method = _description.Value.Operations.FirstOrDefault(operation => operation.UnderlyingMethod == methodInfo);
            if (method == null)
            {
                return null;
            }

            argumentMapping = method.Arguments;
            return method.UrlTemplate;
        }

        /// <inheritdoc />
        ControllerInfo IControllerDescriptionBuilder.BuildDescriptor()
        {
            return _description.Value;
        }

        /// <inheritdoc />
        public ControllerInfo<T> BuildDescriptor()
        {
            return _description.Value;
        }

        /// <summary>Gets the controller route attribute.</summary>
        /// <returns>Controller's route attribute with it's Uri.</returns>
        protected virtual RouteAttribute GetControllerRoute()
        {
            return typeof(T).GetControllerRoute();
        }

        /// <summary>Builds the descriptor internally.</summary>
        /// <returns>Controller information.</returns>
        protected virtual ControllerInfo<T> BuildDescriptorInternal()
        {
            var assembly = (typeof(T).IsGenericType) &&
                (typeof(T).GetGenericArguments()[0].GetInterfaces().Contains(typeof(IController))) ?
                typeof(T).GetGenericArguments()[0].Assembly : typeof(T).Assembly;
            var globalRoutePrefix = assembly.GetCustomAttribute<RouteAttribute>();
            var prefix = GetControllerRoute();
            EntryPointInfo entryPoint = null;
            if (globalRoutePrefix != null)
            {
                prefix = new RouteAttribute(((HttpUrl)prefix.Url).InsertSegments(0, ((HttpUrl)globalRoutePrefix.Url).Segments).ToString());
                entryPoint = new EntryPointInfo(globalRoutePrefix.Url).WithSecurityDetailsFrom(assembly);
            }

            IList<OperationInfo> operations = new List<OperationInfo>();
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Except(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .SelectMany(property => new[] { property.GetGetMethod(), property.GetSetMethod() }))
                .Where(item => item.DeclaringType != typeof(object));
            foreach (var method in methods)
            {
                operations.AddRange(BuildMethodDescriptor(method, prefix));
            }

            return new ControllerInfo<T>(entryPoint, prefix.Url, operations.ToArray()).WithSecurityDetailsFrom(typeof(T));
        }

        private IEnumerable<OperationInfo> BuildMethodDescriptor(MethodInfo method, RouteAttribute prefix)
        {
            bool explicitRoute;
            var verbs = new List<OnVerbAttribute>();
            var route = method.GetDefaults(verbs, out explicitRoute);
            Type entityType = null;
            Type implementation;
            if ((!explicitRoute) && ((implementation = typeof(T).GetInterfaces().FirstOrDefault(@interface => (@interface.IsGenericType) && (typeof(IReadController<,>).IsAssignableFrom(@interface.GetGenericTypeDefinition())))) != null))
            {
                entityType = implementation.GetGenericArguments()[0];
            }

            UriTemplateBuilder templateRegex = new UriTemplateBuilder(entityType, true);
            templateRegex.Add(prefix.Url.ToString(), prefix, typeof(T));
            templateRegex.Add(route.Url.ToString(), route, method);
            Url url = UrlParser.Parse(templateRegex.ToString());
            IList<OperationInfo> result = new List<OperationInfo>();
            foreach (var item in verbs)
            {
                UriTemplateBuilder uriTemplate = templateRegex.DeepCopy(false);
                var parameters = BuildParameterDescriptors(method, item.Verb, templateRegex, ref uriTemplate);
                var regex = new Regex("^" + templateRegex + "$", RegexOptions.IgnoreCase);
                result.Add(new OperationInfo<Verb>(method, url, uriTemplate, regex, item.Verb, parameters).WithSecurityDetailsFrom(method));
            }

            return result;
        }

        private ValueInfo[] BuildParameterDescriptors(MethodInfo method, Verb verb, UriTemplateBuilder templateRegex, ref UriTemplateBuilder uriTemplate)
        {
            IList<ValueInfo> result = new List<ValueInfo>();
            if (method.ReturnParameter.ParameterType != typeof(void))
            {
                result.Add(new ResultInfo(method.ReturnParameter, CreateResultTarget(method.ReturnParameter), null, null));
            }

            foreach (var parameter in method.GetParameters())
            {
                if (parameter.IsOut)
                {
                    result.Add(new ResultInfo(parameter, CreateResultTarget(parameter), null, null));
                    continue;
                }

                bool isBodyParameter;
                string parameterUrlTemplate;
                string parameterTemplateRegex;
                var source = CreateParameterTemplateRegex(method, parameter, verb, out parameterUrlTemplate, out parameterTemplateRegex, out isBodyParameter);
                if ((parameterTemplateRegex == null) && (!isBodyParameter))
                {
                    continue;
                }

                string variableName = (isBodyParameter ? null : UriTemplateBuilder.VariableTemplateRegex.Match(parameterUrlTemplate).Groups["ParameterName"].Value);
                if (parameterTemplateRegex != null)
                {
                    if (!(source is FromQueryStringAttribute))
                    {
                        templateRegex.Add(parameterTemplateRegex, source, parameter, parameter.HasDefaultValue);
                    }

                    uriTemplate.Add(parameterUrlTemplate, source, parameter, parameter.HasDefaultValue);
                }

                result.Add(new ArgumentInfo(parameter, source, (isBodyParameter ? null : (parameterTemplateRegex.StartsWith("([?&]") ? parameterUrlTemplate : uriTemplate.ToString(false))), variableName));
            }

            return result.ToArray();
        }

        private ParameterSourceAttribute CreateParameterTemplateRegex(MethodInfo method, ParameterInfo parameter, Verb verb, out string parameterUrlTemplate, out string parameterTemplateRegex, out bool isBodyParameter)
        {
            parameterUrlTemplate = null;
            isBodyParameter = false;
            var parameterSource = parameter.GetCustomAttribute<ParameterSourceAttribute>(true) ?? _defaultValueRelationSelector.ProvideDefault(parameter, verb);
            var templatedParameterSource = parameterSource as IUrlTemplateParameterSourceAttribute;
            if ((templatedParameterSource != null) && (templatedParameterSource.Template == templatedParameterSource.DefaultTemplate))
            {
                templatedParameterSource = (parameterSource = templatedParameterSource.For(parameter)) as IUrlTemplateParameterSourceAttribute;
            }

            if (templatedParameterSource != null)
            {
                if ((!(parameterUrlTemplate = templatedParameterSource.Template).StartsWith("&")) && 
                    (parameterSource is FromQueryStringAttribute) && (!parameterUrlTemplate.StartsWith("{")))
                {
                    parameterUrlTemplate = "&" + parameterUrlTemplate;
                }
            }

            if ((!parameter.CreateParameterTemplateRegex(parameterSource, out parameterTemplateRegex)) && (parameterSource is FromBodyAttribute))
            {
                isBodyParameter = true;
            }

            return parameterSource;
        }

        private ResultTargetAttribute CreateResultTarget(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<ResultTargetAttribute>(true) ?? _defaultValueRelationSelector.ProvideDefault(parameter);
        }
    }
}