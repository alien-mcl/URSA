using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using URSA.Web.Converters;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Mapping;

namespace URSA.Web.Http
{
    /// <summary>Composes a response.</summary>
    public class ResponseComposer : IResponseComposer
    {
        private static readonly IDictionary<Type, IDictionary<Verb, MethodInfo>> ControllerCrudMethodsCache = new ConcurrentDictionary<Type, IDictionary<Verb, MethodInfo>>();

        private readonly IConverterProvider _converterProvider;
        private readonly Lazy<IEnumerable<ControllerInfo>> _controllerDescriptors;

        /// <summary>Initializes a new instance of the <see cref="ResponseComposer"/> class.</summary>
        /// <param name="converterProvider">The converter provider.</param>
        /// <param name="httpControllerDescriptionBuilders">Controller description builders.</param>
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
                result = (ResponseInfo)output;
                if (result != response)
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

                if (requestMapping.Target.GetType().GetInterfaces()
                    .Any(@interface => (@interface.IsGenericType) && (typeof(IController<>).IsAssignableFrom(@interface.GetGenericTypeDefinition()))))
                {
                    result = HandleCrudRequest(requestMapping, resultingValues, out success);
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

            return result;
        }

        private static IDictionary<Verb, MethodInfo> GetCrudMethods(Type controllerType)
        {
            IDictionary<Verb, MethodInfo> result;
            if (ControllerCrudMethodsCache.TryGetValue(controllerType, out result))
            {
                return result;
            }

            ControllerCrudMethodsCache[controllerType] = result = new Dictionary<Verb, MethodInfo>();
            foreach (var @interface in controllerType.GetInterfaces())
            {
                if (!@interface.IsGenericType)
                {
                    continue;
                }

                var definition = @interface.GetGenericTypeDefinition();
                if (typeof(IController<>).IsAssignableFrom(definition))
                {
                    result[new Verb(String.Empty)] = controllerType.GetInterfaceMap(@interface).TargetMethods.First();
                }
                else if (typeof(IReadController<,>).IsAssignableFrom(definition))
                {
                    result[Verb.GET] = controllerType.GetInterfaceMap(@interface).TargetMethods.First();
                }
                else if (typeof(IWriteController<,>).IsAssignableFrom(definition))
                {
                    var write = @interface;
                    result[Verb.POST] = controllerType.GetInterfaceMap(write).TargetMethods.First(method => method.Name == "Create");
                    result[Verb.PUT] = controllerType.GetInterfaceMap(write).TargetMethods.First(method => method.Name == "Update");
                    result[Verb.DELETE] = controllerType.GetInterfaceMap(write).TargetMethods.First(method => method.Name == "Delete");
                }
            }

            return result;
        }

        private ResponseInfo HandleCrudRequest(IRequestMapping requestMapping, IList<object> resultingValues, out bool success)
        {
            success = true;
            var methods = GetCrudMethods(requestMapping.Target.GetType());
            var method = methods.FirstOrDefault(entry => entry.Value == requestMapping.Operation.UnderlyingMethod);
            if (!Equals(method, default(KeyValuePair<Verb, MethodInfo>)))
            {
                switch (method.Key.ToString())
                {
                    case "":
                        var type = requestMapping.Operation.UnderlyingMethod.ReturnType;
                        return MakeResponse(requestMapping, (resultingValues.Count > 0 ? resultingValues : new[] { new object[0].MakeInstance(type, type.GetItemType()) }));
                    case "GET":
                        return MakeGetResponse(requestMapping, (resultingValues.Count > 0 ? resultingValues[resultingValues.Count - 1] : null));
                    case "POST":
                        return MakePostResponse(requestMapping, (resultingValues.Count > 0 ? (bool?)resultingValues[resultingValues.Count - 1] : null));
                    case "PUT":
                    case "DELETE":
                        return MakeResponse(requestMapping, (resultingValues.Count > 0 ? (bool?)resultingValues[resultingValues.Count - 1] : null));
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
                    result = ObjectResponseInfo<object>.CreateInstance(result.Encoding, result.Request, bodyValues[0].Value.GetType(), bodyValues[0].Value, _converterProvider, result.Headers);
                    break;
                default:
                    result = new MultiObjectResponseInfo(result.Encoding, result.Request, bodyValues.Select(item => item.Value), _converterProvider, result.Headers);
                    break;
            }

            headerValues.ForEach(value => result.Headers.Add(new Header(((ToHeaderAttribute)value.Key.Target).Name, _converterProvider.ConvertFrom(value.Value))));
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

        private ResponseInfo MakePostResponse(IRequestMapping requestMapping, bool? operationResult)
        {
            ResponseInfo result = (ResponseInfo)requestMapping.Target.Response;
            if (operationResult == null)
            {
                result.Status = HttpStatusCode.NotFound;
                return result;
            }

            if (!operationResult.Value)
            {
                result.Status = HttpStatusCode.Found;
                return result;
            }

            var controllerDescriptor = (from controller in _controllerDescriptors.Value
                                       from operation in controller.Operations
                                       where operation.Equals(requestMapping.Operation)
                                       select controller).FirstOrDefault();
            if (controllerDescriptor == null)
            {
                throw new InvalidOperationException(String.Format("Cannot determine the default GET request handler for '{0}'.", requestMapping.Target.GetType()));
            }

            var getMethod = (from operation in controllerDescriptor.Operations
                             where operation.UnderlyingMethod.ReflectedType.GetInterfaceMap(typeof(IReadController<,>)).TargetMethods.Contains(operation.UnderlyingMethod)
                             select operation).First();
            result.Headers.Add(new Header(Header.Location, getMethod.Uri.ToString()));
            return result;
        }

        private ResponseInfo MakeResponse(IRequestMapping requestMapping, bool? operationResult)
        {
            ResponseInfo result = (ResponseInfo)requestMapping.Target.Response;
            if (operationResult == null)
            {
                result.Status = HttpStatusCode.NotFound;
            }
            else if (!operationResult.Value)
            {
                result.Status = HttpStatusCode.Conflict;
            }
            else
            {
                result.Status = HttpStatusCode.NoContent;
            }

            return result;
        }
    }
}