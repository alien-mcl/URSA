using System;
using System.Diagnostics.CodeAnalysis;
using Owin;
using URSA.Configuration;
using URSA.Owin.Handlers;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;

namespace URSA.Owin
{
    /// <summary>Provides methods allowing to integrate URSA framework with an OWIN pipeline.</summary>
    [ExcludeFromCodeCoverage]
    public static class AppExtensions
    {
        private static readonly object Lock = new Object();

        /// <summary>Registers all APIs into the ASP.net pipeline.</summary>
        /// <param name="application">Application to work with.</param>
        public static void RegisterApis(this IAppBuilder application)
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
                application.Use(typeof(UrsaHandler), container.Resolve<IRequestHandler<RequestInfo, ResponseInfo>>());
            }
        }
    }
}