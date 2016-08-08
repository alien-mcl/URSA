using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Composes a response.</summary>
    public class ResponseComposer : IResponseComposer
    {
        private readonly IConverterProvider _converterProvider;
        private readonly Lazy<IEnumerable<ControllerInfo>> _controllerDescriptors;

        /// <summary>Initializes a new instance of the <see cref="ResponseComposer"/> class.</summary>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="httpControllerDescriptionBuilders">Controller description builders.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public ResponseComposer(IConverterProvider converterProvider, IEnumerable<IHttpControllerDescriptionBuilder> httpControllerDescriptionBuilders)
        {
            if (converterProvider == null)
            {
                throw new ArgumentNullException("converterProvider");
            }

            if (httpControllerDescriptionBuilders == null)
            {
                throw new ArgumentNullException("httpControllerDescriptionBuilders");
            }

            _converterProvider = converterProvider;
            _controllerDescriptors = new Lazy<IEnumerable<ControllerInfo>>(() => httpControllerDescriptionBuilders.Select(item => item.BuildDescriptor()));
        }

        /// <inheritdoc />
        public ResponseInfo ComposeResponse(IRequestMapping requestMapping, object output, params object[] arguments)
        {
            if (requestMapping == null)
            {
                throw new ArgumentNullException("requestMapping");
            }

            arguments = arguments ?? new object[0];
            ResponseInfo response = (ResponseInfo)requestMapping.Target.Response;
            ResponseInfo result = null;
            if (output is ResponseInfo)
            {
                if ((result = (ResponseInfo)output) != response)
                {
                    result.Headers.Merge(response.Headers);
                }
            }
            else
            {
                bool success = false;
                var parameters = requestMapping.Operation.UnderlyingMethod.GetParameters();
                var resultingValues = arguments.Where((item, index) => parameters[index].IsOut).ToList();
                if (requestMapping.Operation.UnderlyingMethod.ReturnParameter != null)
                {
                    resultingValues.Add(output);
                }

                if (requestMapping.Target.GetType().GetTypeInfo().GetImplementationOfAny(typeof(IController<>), typeof(IAsyncController<>)) != null)
                {
                    result = HandleCrudRequest(requestMapping, resultingValues, arguments, out success);
                }

                if (!success)
                {
                    result = MakeResponse(requestMapping, resultingValues);
                }
            }

            if (result == response)
            {
                result.Headers.ContentType = StringConverter.TextPlain;
            }

            result.Status = (result.Status == 0 ? HttpStatusCode.OK : result.Status);
            return result;
        }

        private ResponseInfo HandleCrudRequest(IRequestMapping requestMapping, IList<object> resultingValues, object[] arguments, out bool success)
        {
            success = true;
            var methods = requestMapping.Target.GetType().DiscoverCrudMethods();
            var method = methods.FirstOrDefault(entry => entry.Value == requestMapping.Operation.UnderlyingMethod);
            if (!Equals(method, default(KeyValuePair<Verb, MethodInfo>)))
            {
                switch (method.Key.ToString())
                {
                    case "":
                        var type = requestMapping.Operation.UnderlyingMethod.ReturnType.GetTypeInfo();
                        var items = (resultingValues.Count > 1 ? (IEnumerable)resultingValues[1] : new[] { new object[0].MakeInstance(type, type.GetItemType()) });
                        var totalItems = (resultingValues.Count > 0 ? (int)resultingValues[0] : -1);
                        return MakeListResponse(requestMapping, items, arguments, totalItems);
                    case "GET":
                        return MakeGetResponse(requestMapping, (resultingValues.Count > 0 ? resultingValues[resultingValues.Count - 1] : null));
                    case "POST":
                        return MakePostResponse(requestMapping, (resultingValues.Count > 0 ? resultingValues[0] : null));
                    case "PUT":
                    case "DELETE":
                        return MakeDeleteResponse(requestMapping);
                }
            }

            success = false;
            return null;
        }

        private ResponseInfo MakeResponse(IRequestMapping requestMapping, IList<object> resultingValues)
        {
            ResponseInfo result = (ResponseInfo)requestMapping.Target.Response;
            var bodyValues = new List<KeyValuePair<ResultInfo, object>>();
            var headerValues = new List<KeyValuePair<ResultInfo, object>>();
            requestMapping.Operation.Results.ForEach((resultValue, index) =>
                (resultValue.Target is ToHeaderAttribute ? headerValues : bodyValues).Add(new KeyValuePair<ResultInfo, object>(resultValue, resultingValues[index])));

            switch (bodyValues.Count)
            {
                case 0:
                    break;
                case 1:
                    var bodyValue = bodyValues[0];
                    result = ObjectResponseInfo<object>.CreateInstance(result.Encoding, result.Request, bodyValue.Value.GetType(), bodyValue.Value, _converterProvider, result.Headers);
                    break;
                default:
                    result = new MultiObjectResponseInfo(result.Encoding, result.Request, bodyValues.Select(item => item.Value), _converterProvider, result.Headers);
                    break;
            }

            headerValues.ForEach(value => result.Headers.Add(new Header(
                ((ToHeaderAttribute)value.Key.Target).Name,
                String.Format(((ToHeaderAttribute)value.Key.Target).Format, _converterProvider.ConvertFrom(value.Value)))));
            return result;
        }

        private ResponseInfo MakeListResponse(IRequestMapping requestMapping, IEnumerable resultingValues, object[] arguments, int totalItems = 0)
        {
            var status = HttpStatusCode.OK;
            ResponseInfo result = (ResponseInfo)requestMapping.Target.Response;
            if ((arguments[1] != null) && (arguments[2] != null))
            {
                int skip = (int)arguments[1];
                int take = (int)arguments[2];
                take = (take == 0 ? totalItems : Math.Min(take, resultingValues.Cast<object>().Count()));
                var contentRangeHeaderValue = String.Format("members {0}-{1}/{2}", skip, Math.Max(0, take - 1), totalItems);
                result.Headers.Add(new Header("Content-Range", contentRangeHeaderValue));
                status = HttpStatusCode.PartialContent;
            }

            result = ObjectResponseInfo<object>.CreateInstance(
                result.Encoding,
                result.Request,
                requestMapping.Operation.UnderlyingMethod.ReturnType,
                resultingValues,
                _converterProvider,
                result.Headers);
            result.Status = status;
            return result;
        }

        private ResponseInfo MakeGetResponse(IRequestMapping requestMapping, object resultingValue)
        {
            ResponseInfo result = (ResponseInfo)requestMapping.Target.Response;
            if (resultingValue == null)
            {
                result.Status = HttpStatusCode.NotFound;
                return result;
            }

            result = ObjectResponseInfo<object>.CreateInstance(result.Encoding, result.Request, resultingValue.GetType(), resultingValue, _converterProvider, result.Headers);
            return result;
        }

        private ResponseInfo MakePostResponse(IRequestMapping requestMapping, object value)
        {
            ResponseInfo result = (ResponseInfo)requestMapping.Target.Response;
            if (value == null)
            {
                result.Status = HttpStatusCode.BadRequest;
                return result;
            }

            Type controllerType = null;
            var controllerDescriptor = (from controller in _controllerDescriptors.Value
                                        from operation in controller.Operations
                                        where operation.Equals(requestMapping.Operation)
                                        let type = controllerType = operation.UnderlyingMethod.ReflectedType
                                        select controller).FirstOrDefault();
            if (controllerDescriptor == null)
            {
                throw new InvalidOperationException(String.Format("Cannot determine the default GET request handler for '{0}'.", requestMapping.Target.GetType()));
            }

            var getMethod = (from @interface in controllerType.GetInterfaces()
                             where (@interface.IsGenericType) && (@interface.GetGenericTypeDefinition() == typeof(IReadController<,>))
                             from method in controllerType.GetInterfaceMap(@interface).TargetMethods
                             join operation in controllerDescriptor.Operations on method equals operation.UnderlyingMethod
                             select operation).First();
            result.Headers.Add(new Header(Header.Location, getMethod.UrlTemplate.Replace("{" + getMethod.Arguments.First().VariableName + "}", value.ToString())));
            result.Status = HttpStatusCode.Created;
            return result;
        }

        private ResponseInfo MakeDeleteResponse(IRequestMapping requestMapping)
        {
            ResponseInfo result = (ResponseInfo)requestMapping.Target.Response;
            result.Status = HttpStatusCode.NoContent;
            return result;
        }
    }
}