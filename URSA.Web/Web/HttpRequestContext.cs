using System;
using System.Web;

namespace URSA.Web.Web
{
    /// <summary>Represents an HTTP request context.</summary>
    public class HttpRequestContext : IRequestContext
    {
        private readonly HttpContext _context;

        /// <summary>Initializes a new instance of the <see cref="HttpRequestContext"/> class.</summary>
        /// <param name="context">The context.</param>
        public HttpRequestContext(HttpContext context)
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
            get
            {
                return _context.Items[key];
            }

            set
            {
                if (value == null)
                {
                    _context.Items.Remove(key);
                }
                else
                {
                    _context.Items[key] = value;
                }
            }
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
