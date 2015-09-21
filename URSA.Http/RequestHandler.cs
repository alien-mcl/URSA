using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using URSA.Web.Converters;
using URSA.Web.Description.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Serves as the main entry point for the URSA.</summary>
    public class RequestHandler : RequestHandlerBase<RequestInfo, ResponseInfo>
    {
        private readonly IResponseComposer _responseComposer;
        private readonly IArgumentBinder<RequestInfo> _argumentBinder;
        private readonly IDelegateMapper<RequestInfo> _handlerMapper;

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
                ResponseInfo response = new StringResponseInfo(null, request);
                requestMapping.Target.Response = response;
                var arguments = _argumentBinder.BindArguments(request, requestMapping);
                ValidateArguments(requestMapping.Operation.UnderlyingMethod.GetParameters(), arguments);
                object output = requestMapping.Invoke(arguments);
                return _responseComposer.ComposeResponse(requestMapping, output, arguments);
            }
            catch (Exception exception)
            {
                return new ExceptionResponseInfo(request, exception);
            }
        }

        private static void ValidateArguments(ParameterInfo[] parameters, object[] arguments)
        {
            for (int index = 0; index <parameters.Length; index++)
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