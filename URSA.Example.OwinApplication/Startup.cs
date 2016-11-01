using System;
#if CORE
using Microsoft.AspNetCore.Builder;
#else
using Owin;
#endif
using URSA.Example.WebApplication.Security;
using URSA.Owin;

namespace URSA.Example.OwinApplication
{
    /// <summary>Provides an configuration entry point for an Owin pipeline.</summary>
    public class Startup
    {
        /// <summary>Configures the Owin pipeline for URSA.</summary>
        /// <param name="appBuilder">The application builder.</param>
#if CORE
        public void Configure(IApplicationBuilder appBuilder)
#else
        public void Configuration(IAppBuilder appBuilder)
#endif
        {
            if (appBuilder == null)
            {
                throw new ArgumentNullException("appBuilder");
            }

            appBuilder
                .WithCorsEnabled()
                .WithIdentityProvider<BasicIdentityProvider>()
                .WithBasicAuthentication()
                .RegisterApis();
        } 
    }
}