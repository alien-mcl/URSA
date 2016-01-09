using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using URSA.Security;
using URSA.Web.Description;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Serves as the main entry point for the URSA.</summary>
    public class RequestHandler : IRequestHandler<RequestInfo, ResponseInfo>
    {
        private readonly IResponseComposer _responseComposer;
        private readonly IArgumentBinder<RequestInfo> _argumentBinder;
        private readonly IDelegateMapper<RequestInfo> _handlerMapper;

        /// <summary>Initializes a new instance of the <see cref="RequestHandler"/> class.</summary>
        /// <param name="argumentBinder">Argument binder.</param>
        /// <param name="delegateMapper">Delegate mapper.</param>
        /// <param name="responseComposer">Response composer.</param>
        public RequestHandler(IArgumentBinder<RequestInfo> argumentBinder, IDelegateMapper<RequestInfo> delegateMapper, IResponseComposer responseComposer)
        {
            if (argumentBinder == null)
            {
                throw new ArgumentNullException("argumentBinder");
            }

            if (delegateMapper == null)
            {
                throw new ArgumentNullException("delegateMapper");
            }

            if (responseComposer == null)
            {
                throw new ArgumentNullException("responseComposer");
            }

            _responseComposer = responseComposer;
            _argumentBinder = argumentBinder;
            _handlerMapper = delegateMapper;
        }

        /// <inheritdoc />
        public ResponseInfo HandleRequest(RequestInfo request)
        {
            return HandleRequestAsync(request).Result;
        }

        /// <inheritdoc />
        public async Task<ResponseInfo> HandleRequestAsync(RequestInfo request)
        {
            try
            {
                var requestMapping = _handlerMapper.MapRequest(request);
                if (requestMapping == null)
                {
                    throw new NoMatchingRouteFoundException(String.Format("No API resource handles requested url '{0}'.", request.Uri));
                }

                ValidateSecurityRequirements(requestMapping.Operation, request.Identity);
                ResponseInfo response = new StringResponseInfo(null, request);
                requestMapping.Target.Response = response;
                var arguments = _argumentBinder.BindArguments(request, requestMapping);
                ValidateArguments(requestMapping.Operation.UnderlyingMethod.GetParameters(), arguments);
                object output = await ProcessResult(requestMapping.Invoke(arguments));
                return _responseComposer.ComposeResponse(requestMapping, output, arguments);
            }
            catch (Exception exception)
            {
                return new ExceptionResponseInfo(request, exception);
            }
        }

        private static void ValidateSecurityRequirements(OperationInfo operation, IClaimBasedIdentity identity)
        {
            var securityRequirements = operation.UnifiedSecurityRequirements;
            if ((securityRequirements.Denied[ClaimTypes.Anonymous] != null) && (!identity.IsAuthenticated))
            {
                throw new UnauthenticatedAccessException(String.Format("Anonymous access to the requested resource is denied."));
            }

            if (!operation.Allows(identity))
            {
                throw new AccessDeniedException("Access to the requested resource is denied.");
            }
        }

        private static void ValidateArguments(ParameterInfo[] parameters, object[] arguments)
        {
            for (int index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                if ((!parameter.IsOut) && (!parameter.HasDefaultValue) && (arguments[index] == null))
                {
                    throw new ArgumentNullException(parameter.Name);
                }
            }
        }

        private async Task<object> ProcessResult(object output)
        {
            Task task = output as Task;
            if (task == null)
            {
                return output;
            }

            await task;
            if ((task.GetType().IsGenericType) && (typeof(Task<>).IsAssignableFrom(task.GetType().GetGenericTypeDefinition())))
            {
                return task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public).GetValue(task);
            }

            return null;
        }
    }
}