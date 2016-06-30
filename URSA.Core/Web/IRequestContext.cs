using System;

namespace URSA.Web
{
    /// <summary>Represents an abstract request context.</summary>
    public interface IRequestContext : IDisposable
    {
        /// <summary>Raised when the context is about to be disposed.</summary>
        event EventHandler Disposed;

        /// <summary>Gets or sets an item related to the context.</summary>
        /// <param name="key">The key of the item.</param>
        /// <returns>Instance stored under a given <paramref name="key" /> or <b>null</b>.</returns>
        object this[string key] { get; set; }
    }
}
