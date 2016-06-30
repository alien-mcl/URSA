using System;
using System.Web;
using URSA.Web.Web;

namespace URSA.Web.Modules
{
    /// <summary>Provides HTTP request context.</summary>
    public class UrsaModule : IHttpModule
    {
        /// <summary>Defines a key under which a request context is being held in the HTTP context.</summary>
        private const string ContextKey = "URSA.CastleWindsor.RequestContext";

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
            context.Items[ContextKey] = new HttpRequestContext(context);
        }

        private static void OnEndRequest(object sender, EventArgs e)
        {
            var requestContext = ((HttpApplication)sender).Context.Items[ContextKey] as HttpRequestContext;
            if (requestContext != null)
            {
                requestContext.Dispose();
            }
        }
    }
}
