using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Provides a default implementation of the <see cref="IDelegateMapper{RequestInfo}"/> interface.</summary>
    public class DelegateMapper : IDelegateMapper<RequestInfo>
    {
        private readonly Lazy<IEnumerable<ControllerInfo>> _controllerDescriptors;
        private readonly IControllerActivator _controllerActivator;

        /// <summary>Initializes a new instance of the <see cref="DelegateMapper" /> class.</summary>
        /// <param name="httpControllerDescriptionBuilders">HTTP controller description builders.</param>
        /// <param name="controllerActivator">Method creating instances of <see cref="IController" />s.</param>
        [ExcludeFromCodeCoverage]
        public DelegateMapper(IEnumerable<IHttpControllerDescriptionBuilder> httpControllerDescriptionBuilders, IControllerActivator controllerActivator)
        {
            if (httpControllerDescriptionBuilders == null)
            {
                throw new ArgumentNullException("httpControllerDescriptionBuilders");
            }

            if (controllerActivator == null)
            {
                throw new ArgumentNullException("controllerActivator");
            }

            _controllerActivator = controllerActivator;
            _controllerDescriptors = new Lazy<IEnumerable<ControllerInfo>>(() => httpControllerDescriptionBuilders.Select(item => item.BuildDescriptor()));
        }

        /// <inheritdoc />
        IRequestMapping IDelegateMapper.MapRequest(IRequestInfo request)
        {
            if (!(request is RequestInfo))
            {
                throw new ArgumentOutOfRangeException("request");
            }

            return MapRequest((RequestInfo)request);
        }

        /// <inheritdoc />
        public IRequestMapping MapRequest(RequestInfo request)
        {
            var allowedOptions = new List<OperationInfo<Verb>>();
            var methodMismatch = false;
            var possibleOperations = GetPossibleOperations(request);
            foreach (var match in possibleOperations)
            {
                allowedOptions.Add(match.Value);
                if ((request.IsCorsPreflight) && (!typeof(OptionsController).IsAssignableFrom(match.Value.UnderlyingMethod.DeclaringType)))
                {
                    continue;
                }

                if (request.Method != match.Value.ProtocolSpecificCommand)
                {
                    methodMismatch = true;
                    continue;
                }

                var controllerInstance = _controllerActivator.CreateInstance(match.Key.GetType().GetGenericArguments()[0], match.Key.Arguments);
                return new RequestMapping(controllerInstance, match.Value, match.Value.Uri);
            }

            if (allowedOptions.Count == 0)
            {
                return null;
            }

            var option = allowedOptions.First();
            return new OptionsRequestMapping(
                OptionsController.CreateOperationInfo(option),
                option.Uri,
                (methodMismatch ? HttpStatusCode.MethodNotAllowed : HttpStatusCode.OK),
                allowedOptions.Select(item => item.ProtocolSpecificCommand.ToString()).ToArray());
        }

        private IEnumerable<KeyValuePair<ControllerInfo, OperationInfo<Verb>>> GetPossibleOperations(RequestInfo request)
        {
            string requestUri = request.Uri.ToRelativeUri().ToString();
            int indexOf = requestUri.IndexOf('?');
            string requestPath = (indexOf != -1 ? requestUri.Substring(0, indexOf) : requestUri);
            var queryString = (indexOf != -1 ? HttpUtility.ParseQueryString(requestUri.Substring(indexOf)).Cast<string>().Select(key => key.ToLower()).ToArray() : new string[0]);
            return from controller in _controllerDescriptors.Value
                   from operation in controller.Operations
                   where String.Compare(operation.UriTemplate.Split('?')[0], requestPath, true) == 0
                   let rank = (double)GetMatchingArguments(operation, queryString).Count() / queryString.Length
                   orderby rank descending
                   select new KeyValuePair<ControllerInfo, OperationInfo<Verb>>(controller, (OperationInfo<Verb>)operation);
        }

        private IEnumerable<string> GetMatchingArguments(OperationInfo operation, string[] queryString)
        {
            return from argument in operation.Arguments
                   where argument.Source is FromQueryStringAttribute
                   let key = argument.VariableName.ToLower()
                   join queryStringKey in queryString on key equals queryStringKey
                   select key;
        }
    }
}