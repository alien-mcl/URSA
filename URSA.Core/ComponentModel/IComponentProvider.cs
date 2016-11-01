using System;
using System.Collections.Generic;

namespace URSA.ComponentModel
{
    /// <summary>Provides a basic service provider facility interface.</summary>
    public interface IComponentProvider : IComponentComposer, IComponentResolver, IDisposable
    {
        /// <summary>Instructs container to begin a new scope.</summary>
        /// <returns>Component provider of the new scope.</returns>
        IComponentProvider BeginNewScope();

        /// <summary>Instructs the container to dispose a registered instance and drop all scopes bound to that instance.</summary>
        /// <typeparam name="T">Type of the service.</typeparam>
        /// <param name="instance">The instance being disposed.</param>
        void Dispose<T>(T instance);

        /// <summary>Instructs the container to dispose a registered instance and drop all scopes bound to that instance.</summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="instance">The instance being disposed.</param>
        void Dispose(Type serviceType, object instance);
    }
}