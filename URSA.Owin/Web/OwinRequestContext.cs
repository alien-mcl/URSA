using System;
#if CORE
using Microsoft.AspNetCore.Http;
#else
using Microsoft.Owin;
#endif
using URSA.Web;

namespace URSA.Owin.Web
{
    /// <summary>Represents an OWIN request context.</summary>
    public class OwinRequestContext : IRequestContext
    {
#if CORE
        private readonly HttpContext _context;
#else
        private readonly IOwinContext _context;
#endif

        /// <summary>Initializes a new instance of the <see cref="OwinRequestContext"/> class.</summary>
        /// <param name="context">The context.</param>
        public OwinRequestContext(
#if CORE
            HttpContext context)
#else
            IOwinContext context)
#endif
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            _context = context;
        }

        /// <inheritdoc />
        public event EventHandler Disposed;

        /// <inheritdoc />
#if CORE
        public object this[string key]
        {
            get { return _context.Items[key]; }

            set { _context.Items[key] = value; }
        }
#else
        public object this[string key]
        {
            get { return _context.Get<object>(key); }

            set { _context.Set(key, value); }
        }
#endif

        /// <inheritdoc />
        public void Dispose()
        {
            if (Disposed != null)
            {
                Disposed(this, EventArgs.Empty);
            }
        }
    }
}
