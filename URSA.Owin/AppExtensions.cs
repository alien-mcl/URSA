using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Owin;
using URSA.ComponentModel;
using URSA.Configuration;
using URSA.Owin.Handlers;
using URSA.Security;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Security;

namespace URSA.Owin
{
    /// <summary>Provides methods allowing to integrate URSA framework with an OWIN pipeline.</summary>
    [ExcludeFromCodeCoverage]
    public static class AppExtensions
    {
        private static readonly object Lock = new Object();

        /// <summary>Registers all APIs into the ASP.net pipeline.</summary>
        /// <param name="application">The application to work with.</param>
        public static IAppBuilder RegisterApis(this IAppBuilder application)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            lock (Lock)
            {
                var container = UrsaConfigurationSection.InitializeComponentProvider();
                container.Register<IHttpServerConfiguration>(new LazyHttpServerConfiguration());
                container.WithAutodiscoveredControllers();
                application.Use(
                    typeof(UrsaHandler),
                    container.Resolve<IRequestHandler<RequestInfo, ResponseInfo>>(),
                    container.ResolveAll<IAuthenticationProvider>());
            }

            return application;
        }

        /// <summary>Registers the Cross-Origin Resource Sharing component to be used.</summary>
        /// <param name="application">The application to work with.</param>
        /// <param name="allowedOrigins">Allowed origins.</param>
        /// <param name="allowedHeaders">Allowed headers.</param>
        /// <param name="exposedHeaders">Exposed headers.</param>
        /// <returns>The application itself.</returns>
        public static IAppBuilder WithCorsEnabled(
            this IAppBuilder application,
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
            container.Register("CORS", handler);
            return application;
        }

        /// <summary>Registers the Basic authentication mechanism and sets it as a default one.</summary>
        /// <param name="application">The application to work with.</param>
        /// <returns>The application itself.</returns>
        public static IAppBuilder WithBasicAuthentication(this IAppBuilder application)
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.Register<IAuthenticationProvider, BasicAuthenticationProvider>("Basic", lifestyle: Lifestyles.Singleton);
            container.Register<IDefaultAuthenticationScheme, BasicAuthenticationProvider>(lifestyle: Lifestyles.Singleton);
            return application;
        }

        /// <summary>Registers an identity provider.</summary>
        /// <typeparam name="T">Type of the identity provider to use.</typeparam>
        /// <param name="application">The application to work with.</param>
        /// <returns>The application itself.</returns>
        public static IAppBuilder WithIdentityProvider<T>(this IAppBuilder application) where T : IIdentityProvider
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.Register<IIdentityProvider, T>(lifestyle: Lifestyles.Singleton);
            return application;
        }
    }
}