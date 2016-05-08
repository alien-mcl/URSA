using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using RomanticWeb.NamedGraphs;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Security;
using URSA.Web.Configuration;
using URSA.Web.Description;
using URSA.Web.Handlers;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description.NamedGraphs;
using URSA.Web.Http.Security;

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
        /// <param name="application">The application to work with.</param>
        /// <returns>The application itself.</returns>
        public static HttpApplication RegisterApis(this HttpApplication application)
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
                var namedGraphSelector = container.Resolve<INamedGraphSelector>() as ILocallyControlledNamedGraphSelector;
                if (namedGraphSelector != null)
                {
                    namedGraphSelector.CurrentRequest = () => HttpContext.Current.Items["URSA.Http.RequestInfo"] as IRequestInfo;
                }

                routes.ForEach(route => RouteTable.Routes.Add(route.Key, route.Value));
            }

            return application;
        }

        /// <summary>Registers the Cross-Origin Resource Sharing component to be used.</summary>
        /// <param name="application">The application to work with.</param>
        /// <param name="allowedOrigins">Allowed origins.</param>
        /// <param name="allowedHeaders">Allowed headers.</param>
        /// <param name="exposedHeaders">Exposed headers.</param>
        /// <returns>The application itself.</returns>
        public static HttpApplication WithCorsEnabled(
            this HttpApplication application,
            IEnumerable<string> allowedOrigins = null,
            IEnumerable<string> allowedHeaders = null,
            IEnumerable<string> exposedHeaders = null)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            var handler = new CorsPostRequestHandler(
                allowedOrigins ?? CorsPostRequestHandler.WithAny,
                allowedHeaders ?? CorsPostRequestHandler.WithAny,
                exposedHeaders ?? CorsPostRequestHandler.WithAny);
            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.Register<IPostRequestHandler>("CORS", handler);
            return application;
        }

        /// <summary>Registers the Basic authentication mechanism and sets it as a default one.</summary>
        /// <param name="application">The application to work with.</param>
        /// <returns>The application itself.</returns>
        public static HttpApplication WithBasicAuthentication(this HttpApplication application)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.Register<IPreRequestHandler, BasicAuthenticationProvider>("Basic", lifestyle: Lifestyles.Singleton);
            container.Register<IPostRequestHandler, BasicAuthenticationProvider>(lifestyle: Lifestyles.Singleton);
            return application;
        }

        /// <summary>Registers an identity provider.</summary>
        /// <typeparam name="T">Type of the identity provider to use.</typeparam>
        /// <param name="application">The application to work with.</param>
        /// <returns>The application itself.</returns>
        public static HttpApplication WithIdentityProvider<T>(this HttpApplication application) where T : IIdentityProvider
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.Register<IIdentityProvider, T>(lifestyle: Lifestyles.Singleton);
            return application;
        }

        private static IDictionary<string, Route> RegisterApi<T>(IComponentProvider container, ControllerInfo<T> description) where T : IController
        {
            var handler = new UrsaHandler<T>(container.Resolve<IRequestHandler<RequestInfo, ResponseInfo>>());
            string globalRoutePrefix = (description.EntryPoint != null ? description.EntryPoint.Url.ToString().Substring(1) + "/" : String.Empty);
            IDictionary<string, Route> routes = new Dictionary<string, Route>();
            routes[typeof(T).FullName + "DocumentationStylesheet"] = new Route(globalRoutePrefix + EntityConverter.DocumentationStylesheet, handler);
            routes[typeof(T).FullName + "PropertyIcon"] = new Route(globalRoutePrefix + EntityConverter.PropertyIcon, handler);
            routes[typeof(T).FullName + "MethodIcon"] = new Route(globalRoutePrefix + EntityConverter.MethodIcon, handler);
            routes[typeof(T).FullName] = new Route(description.Url.ToString().Substring(1), handler);
            if (!String.IsNullOrEmpty(globalRoutePrefix))
            {
                routes[globalRoutePrefix] = new Route(globalRoutePrefix, handler);
            }

            foreach (var operation in description.Operations)
            {
                string routeTemplate = (operation.UrlTemplate ?? operation.Url.ToString()).Substring(1).Replace("{?", "{");
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