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
        private static readonly IDictionary<Type, IDictionary<Verb, MethodInfo>> ControllerCrudMethodsCache = new ConcurrentDictionary<Type, IDictionary<Verb, MethodInfo>>();

        /// <inheritdoc />
        protected override ResponseInfo HandleRequest(RequestInfo request, IRequestMapping requestMapping)
        {
            try
            {
                ResponseInfo response = new StringResponseInfo(null, request);
                requestMapping.Target.Response = response;
                var arguments = ArgumentBinder.BindArguments(request, requestMapping);
                ValidateArguments(requestMapping.Operation.UnderlyingMethod.GetParameters(), arguments);
                object output = requestMapping.Invoke(arguments);
                return HandleRequestInternal(requestMapping, output, arguments);
            }
            catch (Exception exception)
            {
                return new ExceptionResponseInfo(request, exception);
            }
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            IList<IHttpControllerDescriptionBuilder> builders = new List<IHttpControllerDescriptionBuilder>();
            var controllerTypes = Container.ResolveAllTypes<IController>();
            foreach (var controllerType in controllerTypes)
            {
                builders.Add((IHttpControllerDescriptionBuilder)Container.Resolve(typeof(IHttpControllerDescriptionBuilder<>).MakeGenericType(controllerType)));
            }

            Container.Register<IDelegateMapper<RequestInfo>>(new DelegateMapper(builders, Container.Resolve<IControllerActivator>()));
            Container.Register<IArgumentBinder<RequestInfo>>(new ArgumentBinder(Container.ResolveAll<IParameterSourceArgumentBinder>()));
        }

        private static IDictionary<Verb, MethodInfo> GetCrudMethods(Type controllerType)
        {
            IDictionary<Verb, MethodInfo> result;
            if (!ControllerCrudMethodsCache.TryGetValue(controllerType, out result))
            {
                ControllerCrudMethodsCache[controllerType] = result = new Dictionary<Verb, MethodInfo>();
                foreach (var @interface in controllerType.GetInterfaces())
                {
                    if (@interface.IsGenericType)
                    {
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
                }
            }

            return result;
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

        private ResponseInfo HandleRequestInternal(IRequestMapping requestMapping, object output, object[] arguments)
        {
            ResponseInfo response = (ResponseInfo)requestMapping.Target.Response;
            ResponseInfo responseInfo = null;
            if (output is ResponseInfo)
            {
                responseInfo = (ResponseInfo)output;
                if (responseInfo != response)
                {
                    responseInfo.Headers.Merge(response.Headers);
                }
            }
            else
            {
                bool success = false;
                if (requestMapping.Target.GetType().GetInterfaces()
                    .Any(@interface => (@interface.IsGenericType) && (typeof(IController<>).IsAssignableFrom(@interface.GetGenericTypeDefinition()))))
                {
                    responseInfo = HandleCrudRequest(response, output, requestMapping, arguments, out success);
                }

                if (!success)
                {
                    var parameters = requestMapping.Operation.UnderlyingMethod.GetParameters();
                    var result = new object[] { output }.Concat(arguments.Where((item, index) => parameters[index].IsOut));
                    responseInfo = MakeObjectResponse(requestMapping.Operation.UnderlyingMethod.ReturnType, response, result.ToArray());
                }
            }

            if (responseInfo == response)
            {
                responseInfo.Headers.ContentType = Converters.StringConverter.TextPlain;
            }

            return responseInfo;
        }

        private ResponseInfo HandleCrudRequest(ResponseInfo response, object output, IRequestMapping requestMapping, object[] arguments, out bool success)
        {
            success = true;
            var type = requestMapping.Operation.UnderlyingMethod.ReturnType;
            var methods = GetCrudMethods(requestMapping.Target.GetType());
            var method = methods.FirstOrDefault(entry => entry.Value == requestMapping.Operation.UnderlyingMethod);
            if (!Equals(method, default(KeyValuePair<Verb, MethodInfo>)))
            {
                switch (method.Key.ToString())
                {
                    case "":
                        return MakeObjectResponse(type, response, output ?? (object)Array.CreateInstance(type.GetItemType(), 0));
                    case "GET":
                        return MakeResponseFromValue(type, response, output);
                    case "POST":
                        return MakeResponse(response, requestMapping, arguments, (bool?)output);
                    case "PUT":
                    case "DELETE":
                        return MakeResponse(response, requestMapping, null, (bool?)output);
                }
            }

            success = false;
            return null;
        }

        private ResponseInfo MakeObjectResponseWithOutParameter(ResponseInfo response, IRequestMapping requestMapping, object[] arguments)
        {
            var parameters = requestMapping.Operation.UnderlyingMethod.GetParameters();
            int outIndex = parameters.Length - 1;
            parameters.Where((parameter, index) => (parameter.IsOut) && ((outIndex = index) != 0)).First();
            return MakeObjectResponse(parameters[outIndex].ParameterType, response, arguments[outIndex]);
        }

        private ResponseInfo MakeObjectResponse(Type type, ResponseInfo response, params object[] output)
        {
            ResponseInfo result;
            if (output.Length > 1)
            {
                result = new MultiObjectResponseInfo(response.Encoding, response.Request, output, ConverterProvider, response.Headers);
            }
            else
            {
                result = (ResponseInfo)typeof(ObjectResponseInfo<>).MakeGenericType(type)
                    .GetConstructor(new[] { typeof(Encoding), typeof(RequestInfo), type, typeof(IConverterProvider), typeof(HeaderCollection) })
                    .Invoke(new[] { response.Encoding, response.Request, output[0], ConverterProvider, response.Headers });
            }

            result.Status = HttpStatusCode.OK;
            return result;
        }

        private ResponseInfo MakeResponse(ResponseInfo response, IRequestMapping requestMapping, object[] arguments, bool? status)
        {
            if (status != null)
            {
                if (status.Value)
                {
                    return arguments != null ?
                        MakeObjectResponseWithOutParameter(response, requestMapping, arguments) :
                        MakeResponseFromValue(null, response, null, HttpStatusCode.NoContent);
                }

                return MakeResponseFromValue(null, response, null, HttpStatusCode.Found);
            }
            
            response.Status = HttpStatusCode.NotFound;
            return response;
        }

        private ResponseInfo MakeResponseFromValue(Type type, ResponseInfo response, object output, HttpStatusCode status = HttpStatusCode.NotFound)
        {
            if (output != null)
            {
                return MakeObjectResponse(type, response, output);
            }

            response.Status = status;
            return response;
        }
    }
}