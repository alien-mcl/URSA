using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Description;
using URSA.Web.Description.Http;
using URSA.Web.Handlers;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;

namespace URSA.Web
{
    /// <summary>Provides methods allowing to integrate URSA framework with standard ASP.net pipeline.</summary>
    public static class HttpApplicationExtesions
    {
        /// <summary>Registers all APIs into the ASP.net pipeline.</summary>
        /// <typeparam name="P">Type of protocol for which to register APIs.</typeparam>
        /// <param name="application">Application to work with.</param>
        public static void RegisterApis<P>(this HttpApplication application) where P : IRequestInfo
        {
            var assemblies = UrsaConfigurationSection.GetInstallerAssemblies()
                .Concat(new Assembly[] { Assembly.GetExecutingAssembly() });
            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.RegisterAll<IController>(assemblies);
            var controllers = container.ResolveAllTypes<IController>();
            IDictionary<string, Route> routes = new Dictionary<string, Route>();
            foreach (var controller in controllers)
            {
                if ((!controller.IsGenericType) || ((controller.IsGenericType) && (!typeof(DescriptionController<>).IsAssignableFrom(controller.GetGenericTypeDefinition()))))
                {
                    ((IDictionary<string, Route>)typeof(HttpApplicationExtesions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                        .Where(method => (method.Name == "RegisterApi") && (method.IsGenericMethodDefinition))
                        .Select(method => method.MakeGenericMethod(typeof(P), controller))
                        .First()
                        .Invoke(null, new object[] { container })).ForEach(route =>
                            {
                                if (!routes.ContainsKey(route.Key))
                                {
                                    routes.Add(route.Key, route.Value);
                                }
                            });
                }
            }

            routes.ForEach(route => RouteTable.Routes.Add(route.Key, route.Value));
        }

        /// <summary>Registers a single <typeparamref name="P" /> protocol API.</summary>
        /// <typeparam name="P">Type of protocol for which to register APIs.</typeparam>
        /// <typeparam name="T">Type of controller to register.</typeparam>
        /// <param name="application">Application to work with.</param>
        public static void RegisterApi<P, T>(this HttpApplication application)
            where P : IRequestInfo
            where T : IController
        {
            var routes = RegisterApi<P, T>(UrsaConfigurationSection.InitializeComponentProvider());
            routes.ForEach(route => RouteTable.Routes.Add(route.Key, route.Value));
        }

        private static IDictionary<string, Route> RegisterApi<P, T>(IComponentProvider container)
            where P : IRequestInfo
            where T : IController
        {
            var handler = new UrsaHandler<T>();
            var builder = container.Resolve<IHttpControllerDescriptionBuilder<T>>();
            var description = builder.BuildDescriptor();
            IDictionary<string, Route> routes = new Dictionary<string, Route>();
            routes[typeof(T).FullName + "DocumentationStylesheet"] = new Route(EntityConverter.DocumentationStylesheet, handler);
            routes[typeof(T).FullName] = new Route(description.Uri.ToString().Substring(1), handler);
            foreach (var operation in description.Operations.Cast<OperationInfo>())
            {
                string routeTemplate = (operation.UriTemplate ?? operation.Uri.ToString()).Substring(1).Replace("{?", "{");
                int indexOf = routeTemplate.IndexOf('?');
                if (indexOf != -1)
                {
                    routeTemplate = routeTemplate.Substring(0, indexOf);
                }

                if (routes.All(route => String.Compare(route.Value.Url, routeTemplate, true) != 0))
                {
                    routes[typeof(T).FullName + "." + operation.UnderlyingMethod.Name] = new Route(routeTemplate, handler);
                }
            }

            return routes;
        }
    }
}