using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Description;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Provides a default implementation of the <see cref="IDelegateMapper{RequestInfo}"/> interface.</summary>
    public class DelegateMapper : IDelegateMapper<RequestInfo>
    {
        private static readonly IDictionary<Verb, int> VerbRanks = new Dictionary<Verb, int>()
            {
                { Verb.GET, 100 },
                { Verb.PUT, 90 },
                { Verb.DELETE, 80 },
                { Verb.POST, 70 },
                { Verb.HEAD, 60 },
                { Verb.OPTIONS, 50 },
                { Verb.Empty, 40 }
            };

        private readonly Lazy<IEnumerable<ControllerInfo>> _controllerDescriptors;
        private readonly IControllerActivator _controllerActivator;

        /// <summary>Initializes a new instance of the <see cref="DelegateMapper" /> class.</summary>
        /// <param name="httpControllerDescriptionBuilders">HTTP controller description builders.</param>
        /// <param name="controllerActivator">Method creating instances of <see cref="IController" />s.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
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
            var optionsControllerTypeInfo = typeof(OptionsController).GetTypeInfo();
            foreach (var match in possibleOperations)
            {
                allowedOptions.Add(match.Value);
                if ((request.IsCorsPreflight) && (!optionsControllerTypeInfo.IsAssignableFrom(match.Value.UnderlyingMethod.DeclaringType)))
                {
                    continue;
                }

                if (request.Method != match.Value.ProtocolSpecificCommand)
                {
                    methodMismatch = true;
                    continue;
                }

                var controllerInstance = _controllerActivator.CreateInstance(match.Key.GetType().GetGenericArguments()[0], match.Key.Arguments);
                return new RequestMapping(controllerInstance, match.Value, (HttpUrl)match.Value.Url);
            }

            if (allowedOptions.Count == 0)
            {
                return null;
            }

            var option = allowedOptions.First();
            return new OptionsRequestMapping(
                OptionsController.CreateOperationInfo(option),
                (HttpUrl)option.Url,
                (methodMismatch ? HttpStatusCode.MethodNotAllowed : HttpStatusCode.OK),
                allowedOptions.Select(item => item.ProtocolSpecificCommand.ToString()).ToArray());
        }

        private IEnumerable<KeyValuePair<ControllerInfo, OperationInfo<Verb>>> GetPossibleOperations(RequestInfo request)
        {
            var relativeUrl = request.Url.AsRelative;
            var queryString = (relativeUrl.HasQuery ? relativeUrl.Query.Keys : new string[0]);
            return from controller in _controllerDescriptors.Value
                   from operation in controller.Operations
                   where operation.TemplateRegex.IsMatch(relativeUrl.Path)
                   let httpOperation = (OperationInfo<Verb>)operation
                   let rank = GetMatchRank(controller, httpOperation, queryString, request.Method)
                   orderby rank.MatchingMethod descending, rank.Specialization descending, rank.MatchingRequiredParameters descending, rank.MatchingOptionalParameters descending
                   select new KeyValuePair<ControllerInfo, OperationInfo<Verb>>(controller, httpOperation);
        }

        private Rank GetMatchRank(ControllerInfo controller, OperationInfo<Verb> operation, IEnumerable<string> queryString, Verb method)
        {
            var requiredQueryStringArguments = new List<string>();
            var optionalQueryStringArguments = new List<string>();
            var matchingRequiredParameters = new List<string>();
            var matchingOptionalParameters = new List<string>();
            foreach (var argument in operation.Arguments.Where(argument => argument.Source is FromQueryStringAttribute))
            {
                (argument.Parameter.HasDefaultValue ? optionalQueryStringArguments : requiredQueryStringArguments).Add(argument.VariableName);
                if (queryString.Contains(argument.VariableName, StringComparer.OrdinalIgnoreCase))
                {
                    (argument.Parameter.HasDefaultValue ? matchingOptionalParameters : matchingRequiredParameters).Add(argument.VariableName);
                }
            }

            return new Rank(
                (operation.ProtocolSpecificCommand == method ? (VerbRanks.ContainsKey(method) ? VerbRanks[method] : VerbRanks[Verb.Empty]) : 0),
                (controller.ControllerType.GetTypeInfo().IsGenericType ? 0 : 1),
                (requiredQueryStringArguments.Count != 0 ? matchingRequiredParameters.Count / requiredQueryStringArguments.Count : 0),
                (optionalQueryStringArguments.Count != 0 ? matchingOptionalParameters.Count / optionalQueryStringArguments.Count : 0));
        }

        private struct Rank
        {
            internal readonly int MatchingRequiredParameters;

            internal readonly int MatchingOptionalParameters;

            internal readonly int Specialization;

            internal readonly int MatchingMethod;

            internal Rank(int matchingMethod, int specialization, int matchingRequiredParameters, int matchingOptionalParameters)
            {
                MatchingMethod = matchingMethod;
                Specialization = specialization;
                MatchingRequiredParameters = matchingRequiredParameters;
                MatchingOptionalParameters = matchingOptionalParameters;
            }
        }
    }
}