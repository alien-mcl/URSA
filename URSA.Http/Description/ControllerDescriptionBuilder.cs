using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Http.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Description.Http
{
    /// <summary>Procides an HTTP controller description building facility.</summary>
    /// <typeparam name="T"></typeparam>
    public class ControllerDescriptionBuilder<T> : IHttpControllerDescriptionBuilder<T> where T : IController
    {
        private readonly Lazy<ControllerInfo<T>> _description;
        private readonly IDefaultValueRelationSelector _defaultValueRelationSelector;

        /// <summary>Initializes a new instance of the <see cref="ControllerDescriptionBuilder{T}" /> class.</summary>
        /// <param name="defaultValueRelationSelector">Default parameter source selector.</param>
        [ExcludeFromCodeCoverage]
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
            OperationInfo method = _description.Value.Operations.FirstOrDefault(operation => operation.UnderlyingMethod == methodInfo);
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
            return _description.Value;
        }

        /// <inheritdoc />
        public ControllerInfo<T> BuildDescriptor()
        {
            return _description.Value;
        }

        private ControllerInfo<T> BuildDescriptorInternal()
        {
            var prefix = typeof(T).GetControllerRoute();
            IList<OperationInfo> operations = new List<OperationInfo>();
            var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Except(typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .SelectMany(property => new MethodInfo[] { property.GetGetMethod(), property.GetSetMethod() }))
                .Where(item => item.DeclaringType != typeof(object));
            foreach (var method in methods)
            {
                operations.AddRange(BuildMethodDescriptor(method, prefix));
            }

            return new ControllerInfo<T>(prefix.Uri, operations.ToArray());
        }

        private IEnumerable<OperationInfo> BuildMethodDescriptor(MethodInfo method, RouteAttribute prefix)
        {
            bool explicitRoute;
            var verbs = new List<OnVerbAttribute>();
            var route = method.GetDefaults(verbs, out explicitRoute);
            Type entityType = null;
            Type implementation = null;
            if ((!explicitRoute) && ((implementation = typeof(T).GetInterfaces().FirstOrDefault(@interface => (@interface.IsGenericType) && (typeof(IReadController<,>).IsAssignableFrom(@interface.GetGenericTypeDefinition())))) != null))
            {
                entityType = implementation.GetGenericArguments()[0];
            }

            UriTemplate templateRegex = new UriTemplate(entityType, true);
            templateRegex.Add(prefix.Uri.ToString(), prefix, typeof(T));
            templateRegex.Add(route.Uri.ToString(), route, method);
            Uri uri = new Uri(templateRegex.ToString(), UriKind.RelativeOrAbsolute);
            IList<OperationInfo> result = new List<OperationInfo>();
            foreach (var item in verbs)
            {
                UriTemplate uriTemplate = templateRegex.Clone(false);
                var parameters = BuildParameterDescriptors(method, item.Verb, templateRegex, ref uriTemplate);
                var regex = new Regex("^" + templateRegex + "$", RegexOptions.IgnoreCase);
                result.Add(new OperationInfo<Verb>(method, uri, uriTemplate, regex, item.Verb, parameters));
            }

            return result;
        }

        private ValueInfo[] BuildParameterDescriptors(MethodInfo method, Verb verb, UriTemplate templateRegex, ref UriTemplate uriTemplate)
        {
            bool restUriTemplate = true;
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
                string parameterUriTemplate;
                string parameterTemplateRegex;
                var source = CreateParameterTemplateRegex(method, parameter, verb, out parameterUriTemplate, out parameterTemplateRegex, out isBodyParameter);
                if ((parameterTemplateRegex == null) && (!isBodyParameter))
                {
                    continue;
                }

                string variableName = (isBodyParameter ? null : parameter.Name.ToLowerCamelCase());
                if (parameterTemplateRegex != null)
                {
                    restUriTemplate = false;
                    templateRegex.Add(parameterTemplateRegex, source, parameter, parameter.HasDefaultValue);
                    uriTemplate.Add(parameterUriTemplate, source, parameter, parameter.HasDefaultValue);
                }

                result.Add(new ArgumentInfo(parameter, source, (isBodyParameter ? null : (parameterTemplateRegex[0] == '&' ? parameterUriTemplate : uriTemplate.ToString(false))), variableName));
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
            var parameterSource = parameter.GetCustomAttribute<ParameterSourceAttribute>(true) ?? _defaultValueRelationSelector.ProvideDefault(parameter, verb);
            if ((!parameter.CreateParameterTemplateRegex(parameterSource, out parameterUriTemplate, out parameterTemplateRegex)) && (parameterSource is FromBodyAttribute))
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