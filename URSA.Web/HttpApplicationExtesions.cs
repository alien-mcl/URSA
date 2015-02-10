using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Description;
using URSA.Web.Handlers;

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
            foreach (var controller in controllers)
            {
                typeof(HttpApplicationExtesions).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(method => (method.Name == "RegisterApi") && (method.IsGenericMethodDefinition))
                    .Select(method => method.MakeGenericMethod(typeof(P), controller))
                    .First()
                    .Invoke(null, new object[] { container });
            }
        }

        /// <summary>Registers a single <typeparamref name="P" /> protocol API.</summary>
        /// <typeparam name="P">Type of protocol for which to register APIs.</typeparam>
        /// <typeparam name="T">Type of controller to register.</typeparam>
        /// <param name="application">Application to work with.</param>
        public static void RegisterApi<P, T>(this HttpApplication application)
            where P : IRequestInfo
            where T : IController
        {
            RegisterApi<P, T>(UrsaConfigurationSection.InitializeComponentProvider());
        }

        private static void RegisterApi<P, T>(IComponentProvider container)
            where P : IRequestInfo
            where T : IController
        {
            var builder = container.Resolve<IControllerDescriptionBuilder<T>>();
            var description = builder.BuildDescriptor();
            var handler = new UrsaHandler<T>(description);
            IDictionary<string, Route> routes = new Dictionary<string, Route>();
            routes[typeof(T).FullName] = new Route(description.Uri.ToString().Substring(1) + "/help", handler);
            foreach (var operation in description.Operations.Cast<URSA.Web.Description.Http.OperationInfo>())
            {
                string routeTemplate = (operation.UriTemplate != null ? operation.UriTemplate : operation.Uri.ToString()).Substring(1).Replace("{?", "{");
                int indexOf = routeTemplate.IndexOf('?');
                if (indexOf != -1)
                {
                    routeTemplate = routeTemplate.Substring(0, indexOf);
                }

                if (!routes.Any(route => String.Compare(route.Value.Url, routeTemplate, true) == 0))
                {
                    routes[typeof(T).FullName + "." + operation.UnderlyingMethod.Name] = new Route(routeTemplate, handler);
                }
            }

            routes.ForEach(route => RouteTable.Routes.Add(route.Key, route.Value));
        }
    }
}