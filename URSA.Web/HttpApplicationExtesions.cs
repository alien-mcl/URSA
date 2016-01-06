using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Web.Configuration;
using URSA.Web.Description;
using URSA.Web.Handlers;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Converters;

namespace URSA.Web
{
    /// <summary>Provides methods allowing to integrate URSA framework with standard ASP.net pipeline.</summary>
    [ExcludeFromCodeCoverage]
    public static class HttpApplicationExtesions
    {
        private static readonly MethodInfo RegisterApiT = typeof(HttpApplicationExtesions)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(method => (method.Name == "RegisterApi") && (method.IsGenericMethodDefinition));

        private static readonly object Lock = new Object();

        /// <summary>Registers all APIs into the ASP.net pipeline.</summary>
        /// <param name="application">Application to work with.</param>
        public static void RegisterApis(this HttpApplication application)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            lock (Lock)
            {
                var routes = new Dictionary<string, Route>();
                var container = UrsaConfigurationSection.InitializeComponentProvider();
                container.Register<IHttpServerConfiguration, HttpContextBoundServerConfiguration>();
                container.WithAutodiscoveredControllers((controller, description) =>
                    {
                        var routesToAdd = (IDictionary<string, Route>)RegisterApiT.MakeGenericMethod(controller).Invoke(null, new object[] { container, description });
                        routes.AddRange(routesToAdd.Where(route => !routes.ContainsKey(route.Key)));
                    });

                routes.ForEach(route => RouteTable.Routes.Add(route.Key, route.Value));
            }
        }

        private static IDictionary<string, Route> RegisterApi<T>(IComponentProvider container, ControllerInfo<T> description) where T : IController
        {
            var handler = new UrsaHandler<T>(container.Resolve<IRequestHandler<RequestInfo, ResponseInfo>>());
            string globalRoutePrefix = (description.EntryPoint != null ? description.EntryPoint.ToString().Substring(1) + "/" : String.Empty);
            IDictionary<string, Route> routes = new Dictionary<string, Route>();
            routes[typeof(T).FullName + "DocumentationStylesheet"] = new Route(globalRoutePrefix + EntityConverter.DocumentationStylesheet, handler);
            routes[typeof(T).FullName + "PropertyIcon"] = new Route(globalRoutePrefix + EntityConverter.PropertyIcon, handler);
            routes[typeof(T).FullName + "MethodIcon"] = new Route(globalRoutePrefix + EntityConverter.MethodIcon, handler);
            routes[typeof(T).FullName] = new Route(description.Uri.ToString().Substring(1), handler);
            if (!String.IsNullOrEmpty(globalRoutePrefix))
            {
                routes[globalRoutePrefix] = new Route(globalRoutePrefix, handler);
            }

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