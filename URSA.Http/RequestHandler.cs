using System;
using System.Reflection;
using System.Threading.Tasks;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Serves as the main entry point for the URSA.</summary>
    public class RequestHandler : RequestHandlerBase<RequestInfo, ResponseInfo>
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
        public override ResponseInfo HandleRequest(RequestInfo request)
        {
            try
            {
                var requestMapping = _handlerMapper.MapRequest(request);
                if (requestMapping == null)
                {
                    throw new NotFoundException(String.Format("No API resource handles requested url '{0}'.", request.Uri));
                }

                ResponseInfo response = new StringResponseInfo(null, request);
                requestMapping.Target.Response = response;
                var arguments = _argumentBinder.BindArguments(request, requestMapping);
                ValidateArguments(requestMapping.Operation.UnderlyingMethod.GetParameters(), arguments);
                object output = HandleRequestInternal(requestMapping, arguments);
                return _responseComposer.ComposeResponse(requestMapping, output, arguments);
            }
            catch (Exception exception)
            {
                return new ExceptionResponseInfo(request, exception);
            }
        }

        private static object HandleRequestInternal(IRequestMapping requestMapping, object[] arguments)
        {
            object output = null;
            Task.Run(async () =>
                {
                    object handlerResult = requestMapping.Invoke(arguments);
                    if (handlerResult == null)
                    {
                        return;
                    }

                    Task task = handlerResult as Task;
                    if (task == null)
                    {
                        output = handlerResult;
                        return;
                    }

                    if ((task.GetType().IsGenericType) && (typeof(Task<>).IsAssignableFrom(task.GetType().GetGenericTypeDefinition())))
                    {
                        output = task.GetType().GetProperty("Result", BindingFlags.Instance | BindingFlags.Public).GetValue(task);
                        return;
                    }

                    await task;
                }).Wait();

            return output;
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
    }
}