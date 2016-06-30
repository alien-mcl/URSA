using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Lifestyle.Scoped;
using URSA.Web;

namespace Castle.MicroKernel.Lifestyle
{
    /// <summary>Provides an universal web request scope.</summary>
    public class UniversalWebRequestScopeAccessor : IScopeAccessor
    {
        private const string Key = "URSA.CastleWindsor.RequestContextScope";
        private ILifetimeScope _temporaryScope = null;

        /// <inheritdoc />
        public void Dispose()
        {
            var requestContext = System.Runtime.Remoting.Messaging.CallContext.HostContext;
            if (requestContext == null)
            {
                if (_temporaryScope != null)
                {
                    _temporaryScope.Dispose();
                }

                return;
            }

            var universalContext = requestContext as IRequestContext;
            if (universalContext != null)
            {
                DisposeUniversalContext(universalContext);
                return;
            }

            DisposeHttpContext(requestContext as HttpContext);
        }

        /// <inheritdoc />
        public ILifetimeScope GetScope(CreationContext context)
        {
            var requestContext = System.Runtime.Remoting.Messaging.CallContext.HostContext;
            if (requestContext == null)
            {
                return _temporaryScope ?? (_temporaryScope = new DefaultLifetimeScope(new ScopeCache()));
            }

            var owinContext = requestContext as IRequestContext;
            return (owinContext != null ? GetUniversalContext(owinContext) : GetHttpContext(requestContext as HttpContext));
        }

        private void DisposeUniversalContext(IRequestContext context)
        {
            var scope = (ILifetimeScope)context[Key];
            if (scope == null)
            {
                return;
            }

            context[Key] = null;
            scope.Dispose();
        }

        private void DisposeHttpContext(HttpContext context)
        {
            var requestContext = (from entry in context.Items.Cast<DictionaryEntry>()
                                  where entry.Value is IRequestContext
                                  select new KeyValuePair<string, IRequestContext>((string)entry.Key, (IRequestContext)entry.Value)).FirstOrDefault();
            if (default(KeyValuePair<string, IRequestContext>).Equals(requestContext))
            {
                return;
            }

            DisposeUniversalContext(requestContext.Value);
            context.Items.Remove(requestContext.Key);
        }

        private ILifetimeScope GetUniversalContext(IRequestContext context)
        {
            var scope = (ILifetimeScope)context[Key];
            if (scope == null)
            {
                scope = new DefaultLifetimeScope(new ScopeCache());
                context[Key] = scope;
                context.Disposed += (sender, e) =>
                    {
                        context[Key] = null;
                        scope.Dispose();
                    };
            }

            return scope;
        }

        private ILifetimeScope GetHttpContext(HttpContext context)
        {
            var requestContext = (from entry in context.Items.Cast<DictionaryEntry>()
                                  where entry.Value is IRequestContext
                                  select new KeyValuePair<string, IRequestContext>((string)entry.Key, (IRequestContext)entry.Value)).FirstOrDefault();
            if (default(KeyValuePair<string, IRequestContext>).Equals(requestContext))
            {
                return _temporaryScope ?? (_temporaryScope = new DefaultLifetimeScope(new ScopeCache()));
            }

            return GetUniversalContext(requestContext.Value);
        }
    }
}
