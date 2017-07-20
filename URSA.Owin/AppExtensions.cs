using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if CORE
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
#else
using Microsoft.Owin;
using Owin;
#endif
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
        /// <returns>Application builder.</returns>
#if CORE
        public static IApplicationBuilder RegisterApis(this IApplicationBuilder application)
#else
        public static IAppBuilder RegisterApis(this IAppBuilder application)
#endif
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            IComponentProvider container;
            lock (Lock)
            {
                container = UrsaConfigurationSection.InitializeComponentProvider();
                container.Register<IHttpServerConfiguration>(new LazyHttpServerConfiguration());
                container.WithAutodiscoveredControllers();
            }

            application.Use((context, next) =>
                {
                    if (LazyHttpServerConfiguration.HostingUri == null)
                    {
                        LazyHttpServerConfiguration.HostingUri = GetHostingUri(context);
                    }

                    using (IComponentProvider requestScopeContainer = container.BeginNewScope())
                    {
                        var handler = new UrsaHandler(next, requestScopeContainer.Resolve<IRequestHandler<RequestInfo, ResponseInfo>>());
                        return handler.Invoke(context);
                    }
                });

            return application;
        }

#pragma warning disable SA1115 // Parameter must follow comma
        /// <summary>Registers the Cross-Origin Resource Sharing component to be used.</summary>
        /// <param name="application">The application to work with.</param>
        /// <param name="allowedOrigins">Allowed origins.</param>
        /// <param name="allowedHeaders">Allowed headers.</param>
        /// <param name="exposedHeaders">Exposed headers.</param>
        /// <returns>The application itself.</returns>
#if CORE
        public static IApplicationBuilder WithCorsEnabled(
            this IApplicationBuilder application,
#else
        public static IAppBuilder WithCorsEnabled(
            this IAppBuilder application,
#endif
            IEnumerable<string> allowedOrigins = null,
            IEnumerable<string> allowedHeaders = null,
            IEnumerable<string> exposedHeaders = null)
#pragma warning restore SA1115 // Parameter must follow comma
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
#if CORE
        public static IApplicationBuilder WithBasicAuthentication(this IApplicationBuilder application)
#else
        public static IAppBuilder WithBasicAuthentication(this IAppBuilder application)
#endif
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
#if CORE
        public static IApplicationBuilder WithIdentityProvider<T>(this IApplicationBuilder application) where T : IIdentityProvider
#else
        public static IAppBuilder WithIdentityProvider<T>(this IAppBuilder application) where T : IIdentityProvider
#endif
        {
            if (application == null)
            {
                throw new ArgumentNullException("application");
            }

            var container = UrsaConfigurationSection.InitializeComponentProvider();
            container.Register<IIdentityProvider, T>(lifestyle: Lifestyles.Singleton);
            return application;
        }

#if CORE
        private static Uri GetHostingUri(HttpContext context)
        {
            return new Uri(String.Format("{0}://{1}/", context.Request.Scheme, context.Request.Host));
        }
#else
        private static Uri GetHostingUri(IOwinContext context)
        {
            return new Uri(context.Request.Uri.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
        }
#endif
    }
}