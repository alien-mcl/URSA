using System;
using System.Web;
using URSA.Configuration;

namespace URSA.Web.Modules
{
    /// <summary>Provides HTTP request context.</summary>
    public class UrsaModule : IHttpModule
    {
        /// <summary>Defines a key under which a request context is being held in the HTTP context.</summary>
        internal const string ContextKey = "URSA.RequestScope";

        /// <inheritdoc />
        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
            context.EndRequest += OnEndRequest;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private static void OnBeginRequest(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            context.Items[ContextKey] = UrsaConfigurationSection.InitializeComponentProvider().BeginNewScope();
        }

        private static void OnEndRequest(object sender, EventArgs e)
        {
            var scope = ((HttpApplication)sender).Context.Items[ContextKey] as IDisposable;
            if (scope != null)
            {
                scope.Dispose();
            }
        }
    }
}
