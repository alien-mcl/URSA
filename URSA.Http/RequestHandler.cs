using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using URSA.Security;
using URSA.Web.Description;
using URSA.Web.Http.Security;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Serves as the main entry point for the URSA.</summary>
    public class RequestHandler : IRequestHandler<RequestInfo, ResponseInfo>
    {
        private readonly IResponseComposer _responseComposer;
        private readonly IArgumentBinder<RequestInfo> _argumentBinder;
        private readonly IDelegateMapper<RequestInfo> _handlerMapper;
        private readonly IPreRequestHandler[] _preRequestHandlers;
        private readonly IPostRequestHandler[] _postRequestHandlers;
        private readonly IDefaultAuthenticationScheme _defaultAuthenticationScheme;

        /// <summary>Initializes a new instance of the <see cref="RequestHandler"/> class.</summary>
        /// <param name="argumentBinder">Argument binder.</param>
        /// <param name="delegateMapper">Delegate mapper.</param>
        /// <param name="responseComposer">Response composer.</param>
        /// <param name="preRequestHandlers">Handlers executed before the request is processed.</param>
        /// <param name="postRequestHandlers">Handlers executed after the request is processed.</param>
        /// <param name="defaultAuthenticationScheme">Default authentication scheme selector.</param>
        public RequestHandler(
            IArgumentBinder<RequestInfo> argumentBinder,
            IDelegateMapper<RequestInfo> delegateMapper,
            IResponseComposer responseComposer,
            IEnumerable<IPreRequestHandler> preRequestHandlers,
            IEnumerable<IPostRequestHandler> postRequestHandlers,
            IDefaultAuthenticationScheme defaultAuthenticationScheme)
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
            _preRequestHandlers = (preRequestHandlers ?? new IPreRequestHandler[0]).ToArray();
            _postRequestHandlers = (postRequestHandlers ?? new IPostRequestHandler[0]).ToArray();
            _defaultAuthenticationScheme = defaultAuthenticationScheme;
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
                ProcessPreRequestHandlers(request);
                object output = await ProcessResult(requestMapping.Invoke(arguments));
                response = _responseComposer.ComposeResponse(requestMapping, output, arguments);
                ProcessPostRequestHandlers(response);
                return response;
            }
            catch (UnauthenticatedAccessException exception)
            {
                if (_defaultAuthenticationScheme == null)
                {
                    throw new InvalidOperationException("Cannot perform authentication without default authentication scheme set for challenge.");
                }

                var result = new ExceptionResponseInfo(request, exception);
                _defaultAuthenticationScheme.Challenge(result);
                return result;
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
                throw new UnauthenticatedAccessException("Anonymous access to the requested resource is denied.");
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

        private void ProcessPreRequestHandlers(RequestInfo requestInfo)
        {
            Task[] handlers = new Task[_preRequestHandlers.Length];
            for (int index = 0; index < _preRequestHandlers.Length; index++)
            {
                handlers[index] = _preRequestHandlers[index].Process(requestInfo);
            }

            Task.WaitAll(handlers);
        }

        private void ProcessPostRequestHandlers(ResponseInfo responseInfo)
        {
            Task[] handlers = new Task[_postRequestHandlers.Length];
            for (int index = 0; index < _postRequestHandlers.Length; index++)
            {
                handlers[index] = _postRequestHandlers[index].Process(responseInfo);
            }

            Task.WaitAll(handlers);
        }
    }
}