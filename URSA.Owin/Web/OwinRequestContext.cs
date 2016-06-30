using System;
using Microsoft.Owin;
using URSA.Web;

namespace URSA.Owin.Web
{
    /// <summary>Represents an OWIN request context.</summary>
    public class OwinRequestContext : IRequestContext
    {
        private readonly IOwinContext _context;

        /// <summary>Initializes a new instance of the <see cref="OwinRequestContext"/> class.</summary>
        /// <param name="context">The context.</param>
        public OwinRequestContext(IOwinContext context)
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
        public object this[string key]
        {
            get { return _context.Get<object>(key); }

            set { _context.Set(key, value); }
        }

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
