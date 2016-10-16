using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace URSA.Web.Http.Description
{
    internal static class ControllerTypeExtensions
    {
        private static readonly IEnumerable<HttpStatusCode> DefaultStatusCodes = new HttpStatusCode[0]; 
        private static readonly IDictionary<Type, IDictionary<Verb, MethodInfo>> ControllerCrudMethodsCache = new ConcurrentDictionary<Type, IDictionary<Verb, MethodInfo>>();

        private static readonly IDictionary<Verb, IEnumerable<HttpStatusCode>> ControllerCrudMethodStatusCodes = new Dictionary<Verb, IEnumerable<HttpStatusCode>>()
            {
                { Verb.Empty, new[] { HttpStatusCode.OK, HttpStatusCode.PartialContent } },
                { Verb.GET, new[] { HttpStatusCode.OK } },
                { Verb.POST, new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest } },
                { Verb.PUT, new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Conflict } },
                { Verb.DELETE, new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound } }
            };

        internal static IDictionary<Verb, MethodInfo> DiscoverCrudMethods(this Type controllerType)
        {
            IDictionary<Verb, MethodInfo> result;
            lock (ControllerCrudMethodStatusCodes)
            {
                if (ControllerCrudMethodsCache.TryGetValue(controllerType, out result))
                {
                    return result;
                }

                ControllerCrudMethodsCache[controllerType] = result = new Dictionary<Verb, MethodInfo>();
                foreach (var @interface in controllerType.GetInterfaces())
                {
                    if (!@interface.GetTypeInfo().IsGenericType)
                    {
                        continue;
                    }

                    var definition = @interface.GetGenericTypeDefinition();
                    if ((typeof(IWriteController<,>).IsAssignableFrom(definition)) || (typeof(IAsyncWriteController<,>).IsAssignableFrom(definition)))
                    {
                        var write = @interface;
                        var interfaceMap = controllerType.GetTypeInfo().GetRuntimeInterfaceMap(write);
                        result[Verb.POST] = interfaceMap.TargetMethods.First(method => method.Name.StartsWith("Create"));
                        result[Verb.PUT] = interfaceMap.TargetMethods.First(method => method.Name.StartsWith("Update"));
                        result[Verb.DELETE] = interfaceMap.TargetMethods.First(method => method.Name.StartsWith("Delete"));
                    }
                    else if ((typeof(IReadController<,>).IsAssignableFrom(definition)) || (typeof(IAsyncReadController<,>).IsAssignableFrom(definition)))
                    {
                        result[Verb.GET] = controllerType.GetTypeInfo().GetRuntimeInterfaceMap(@interface).TargetMethods.First();
                    }
                    else if ((typeof(IController<>).IsAssignableFrom(definition)) || (typeof(IAsyncController<>).IsAssignableFrom(definition)))
                    {
                        result[Verb.Empty] = controllerType.GetTypeInfo().GetRuntimeInterfaceMap(@interface).TargetMethods.First();
                    }
                }
            }

            return result;
        }

        internal static IEnumerable<HttpStatusCode> DiscoverCrudStatusCodes(this MethodInfo method, Verb verb, Type controllerType = null)
        {
            var methods = (controllerType ?? method.DeclaringType).DiscoverCrudMethods();
            MethodInfo matchedMethod;
            Verb matchedVerb;
            if (((!methods.TryGetValue(matchedVerb = verb, out matchedMethod)) || (matchedMethod != method)) &&
                ((!methods.TryGetValue(matchedVerb = Verb.Empty, out matchedMethod)) || (matchedMethod != method)))
            {
                return DefaultStatusCodes;
            }

            return ControllerCrudMethodStatusCodes[matchedVerb];
        }

        internal static IEnumerable<int> DiscoverCrudStatusCodeNumbers(this MethodInfo method, Verb verb, Type controllerType = null)
        {
            return method.DiscoverCrudStatusCodes(verb, controllerType).Select(httpStatusCode => (int)httpStatusCode).Distinct();
        }
    }
}