using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private readonly ICollection<IPreRequestHandler> _authenticationProviders;
        private readonly ICollection<IPreRequestHandler> _preRequestHandlers;
        private readonly ICollection<IPostRequestHandler> _postRequestHandlers;
        private readonly IEnumerable<IModelTransformer> _modelTransformers;
        private readonly IPostRequestHandler _defaultAuthenticationScheme;

        /// <summary>Initializes a new instance of the <see cref="RequestHandler"/> class.</summary>
        /// <param name="argumentBinder">Argument binder.</param>
        /// <param name="delegateMapper">Delegate mapper.</param>
        /// <param name="responseComposer">Response composer.</param>
        /// <param name="preRequestHandlers">Handlers executed before the request is processed.</param>
        /// <param name="postRequestHandlers">Handlers executed after the request is processed.</param>
        /// <param name="modelTransformers">Model transformers executed after the request is processed and just before the post handlers are invoked.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public RequestHandler(
            IArgumentBinder<RequestInfo> argumentBinder,
            IDelegateMapper<RequestInfo> delegateMapper,
            IResponseComposer responseComposer,
            IEnumerable<IPreRequestHandler> preRequestHandlers,
            IEnumerable<IPostRequestHandler> postRequestHandlers,
            IEnumerable<IModelTransformer> modelTransformers)
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
            _preRequestHandlers = new List<IPreRequestHandler>();
            _postRequestHandlers = new List<IPostRequestHandler>();
            _authenticationProviders = new List<IPreRequestHandler>();
            _modelTransformers = modelTransformers ?? new IModelTransformer[0];
            _defaultAuthenticationScheme = Initialize(preRequestHandlers, postRequestHandlers);
        }

        /// <inheritdoc />
        public ResponseInfo HandleRequest(RequestInfo request)
        {
            return HandleRequestAsync(request).Result;
        }

        /// <inheritdoc />
        public async Task<ResponseInfo> HandleRequestAsync(RequestInfo request)
        {
            ResponseInfo response;
            try
            {
                var requestMapping = _handlerMapper.MapRequest(request);
                if (requestMapping == null)
                {
                    throw new NoMatchingRouteFoundException(String.Format("No API resource handles requested url '{0}'.", request.Url));
                }

                await ProcessAuthenticationProviders(request);
                ValidateSecurityRequirements(requestMapping.Operation, request.Identity);
                response = new StringResponseInfo(null, request);
                requestMapping.Target.Response = response;
                var arguments = _argumentBinder.BindArguments(request, requestMapping);
                ValidateArguments(requestMapping.Operation.UnderlyingMethod.GetParameters(), arguments);
                await ProcessPreRequestHandlers(request);
                object output = await ProcessResult(requestMapping.Invoke(arguments));
                output = await ProcessModelTransformers(requestMapping, request, output, arguments);
                response = _responseComposer.ComposeResponse(requestMapping, output, arguments);
                await ProcessPostRequestHandlers(response);
                return response;
            }
            catch (UnauthenticatedAccessException exception)
            {
                if (_defaultAuthenticationScheme == null)
                {
                    throw new InvalidOperationException("Cannot perform authentication without default authentication scheme set for challenge.");
                }

                response = new ExceptionResponseInfo(request, exception);
            }
            catch (Exception exception)
            {
                response = new ExceptionResponseInfo(request, exception);
            }

            await ProcessPostRequestHandlers(response);
            if (((ExceptionResponseInfo)response).Value is UnauthenticatedAccessException)
            {
                await _defaultAuthenticationScheme.Process(response);
            }

            return response;
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

        private async Task ProcessAuthenticationProviders(RequestInfo requestInfo)
        {
            if (String.IsNullOrEmpty(requestInfo.Headers.Authorization))
            {
                return;
            }

            await ProcessPreRequestHandlers(requestInfo, _authenticationProviders);
        }

        private async Task ProcessPreRequestHandlers(RequestInfo requestInfo, IEnumerable<IPreRequestHandler> preRequestHandlers = null)
        {
            preRequestHandlers = preRequestHandlers ?? _preRequestHandlers;
            foreach (IPreRequestHandler preRequestHandler in preRequestHandlers)
            {
                await preRequestHandler.Process(requestInfo);
            }
        }

        private async Task ProcessPostRequestHandlers(ResponseInfo responseInfo)
        {
            foreach (IPostRequestHandler postRequestHandler in _postRequestHandlers)
            {
                await postRequestHandler.Process(responseInfo);
            }
        }

        private async Task<object> ProcessModelTransformers(IRequestMapping requestMapping, RequestInfo requestInfo, object result, object[] arguments)
        {
            foreach (IModelTransformer modelTransformer in _modelTransformers)
            {
                result = await modelTransformer.Transform(requestMapping, requestInfo, result, arguments);
            }

            return result;
        }

        private IDefaultAuthenticationScheme Initialize(IEnumerable<IPreRequestHandler> preRequestHandlers, IEnumerable<IPostRequestHandler> postRequestHandlers)
        {
            IDefaultAuthenticationScheme result = null;
            foreach (var preRequestHandler in (preRequestHandlers ?? new IPreRequestHandler[0]))
            {
                (preRequestHandler is IAuthenticationProvider ? _authenticationProviders : _preRequestHandlers).Add(preRequestHandler);
            }

            foreach (var postRequestHandler in (postRequestHandlers ?? new IPostRequestHandler[0]))
            {
                var defaultAuthenticationScheme = postRequestHandler as IDefaultAuthenticationScheme;
                if (defaultAuthenticationScheme != null)
                {
                    if (result != null)
                    {
                        throw new InvalidOperationException("Multiple default authentication schemes encountered.");
                    }

                    result = defaultAuthenticationScheme;
                }
                else
                {
                    _postRequestHandlers.Add(postRequestHandler);
                }
            }

            return result;
        }
    }
}