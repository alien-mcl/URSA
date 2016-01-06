using System;
using Owin;
using URSA.Owin;

namespace URSA.Example.OwinApplication
{
    /// <summary>Provides an configuration entry point for an Owin pipeline.</summary>
    public class Startup
    {
        /// <summary>Configures the Owin pipeline for URSA.</summary>
        /// <param name="appBuilder">The application builder.</param>
        public void Configuration(IAppBuilder appBuilder)
        {
            if (appBuilder == null)
            {
                throw new ArgumentNullException("appBuilder");
            }

            appBuilder.RegisterApis();
        } 
    }
}