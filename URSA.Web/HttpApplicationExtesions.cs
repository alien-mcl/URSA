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
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Mapping;

namespace URSA.Web
{
    /// <summary>Provides methods allowing to integrate URSA framework with standard ASP.net pipeline.</summary>
    public static class HttpApplicationExtesions
    {
        /// <summary>Registers all APIs into the ASP.net pipeline.</summary>
        /// <param name="application">Application to work with.</param>
        public static void RegisterApis(this HttpApplication application)
        {
            var assemblies = UrsaConfigurationSection.GetInstallerAssemblies().Concat(new Assembly[] { Assembly.GetExecutingAssembly() });
            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.RegisterAll<IController>(assemblies);
            var controllers = container.ResolveAllTypes<IController>();
            controllers
                .Where(controllerType => (!controllerType.IsGenericType) || ((controllerType.IsGenericType) && (controllerType.GetGenericTypeDefinition() != typeof(DescriptionController<>))))
                .ForEach(controllerType => container.Register(
                typeof(IHttpControllerDescriptionBuilder<>).MakeGenericType(controllerType),
                typeof(ControllerDescriptionBuilder<>).MakeGenericType(controllerType),
                typeof(IHttpControllerDescriptionBuilder<>).MakeGenericType(controllerType).FullName));
            controllers.ForEach(controllerType => container.Register(
                typeof(IHttpControllerDescriptionBuilder),
                typeof(ControllerDescriptionBuilder<>).MakeGenericType(controllerType)));
            IDictionary<string, Route> routes = new Dictionary<string, Route>();
            foreach (var controller in controllers.Where(controller => (!controller.IsGenericType) || 
                ((controller.IsGenericType) && (!typeof(DescriptionController<>).IsAssignableFrom(controller.GetGenericTypeDefinition())))))
            {
                ((IDictionary<string, Route>)typeof(HttpApplicationExtesions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(method => (method.Name == "RegisterApi") && (method.IsGenericMethodDefinition))
                    .Select(method => method.MakeGenericMethod(controller))
                    .First()
                    .Invoke(null, new object[] { container })).ForEach(route =>
                    {
                        if (!routes.ContainsKey(route.Key))
                        {
                            routes.Add(route.Key, route.Value);
                        }
                    });
            }

            routes.ForEach(route => RouteTable.Routes.Add(route.Key, route.Value));
        }

        /// <summary>Registers a single HTTP protocol API.</summary>
        /// <typeparam name="T">Type of controller to register.</typeparam>
        /// <param name="application">Application to work with.</param>
        public static void RegisterApi<T>(this HttpApplication application)
            where T : IController
        {
            var routes = RegisterApi<T>(UrsaConfigurationSection.InitializeComponentProvider());
            routes.ForEach(route => RouteTable.Routes.Add(route.Key, route.Value));
        }

        private static IDictionary<string, Route> RegisterApi<T>(IComponentProvider container)
            where T : IController
        {
            var handler = new UrsaHandler<T>(container.Resolve<IRequestHandler<RequestInfo, ResponseInfo>>());
            var builder = container.Resolve<IHttpControllerDescriptionBuilder<T>>();
            var description = builder.BuildDescriptor();
            IDictionary<string, Route> routes = new Dictionary<string, Route>();
            routes[typeof(T).FullName + "DocumentationStylesheet"] = new Route(EntityConverter.DocumentationStylesheet, handler);
            routes[typeof(T).FullName + "PropertyIcon"] = new Route(EntityConverter.PropertyIcon, handler);
            routes[typeof(T).FullName + "MethodIcon"] = new Route(EntityConverter.MethodIcon, handler);
            routes[typeof(T).FullName] = new Route(description.Uri.ToString().Substring(1), handler);
            foreach (var operation in description.Operations)
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