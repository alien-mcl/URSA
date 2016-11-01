using System;
using System.Collections.Generic;

namespace URSA.ComponentModel
{
    /// <summary>Provides a basic service resolution facility interface.</summary>
    public interface IComponentResolver
    {
        /// <summary>Gets a value indicating whether this is a root component provider.</summary>
        bool IsRoot { get; }

        /// <summary>Determines whether it is possible to resolve the instance of type <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type of instance to resolve.</typeparam>
        /// <param name="arguments">Optional constructor arguments.</param>
        /// <returns><b>true</b> if it is possible to resolve the instance; otherwise <b>false</b>.</returns>
        bool CanResolve<T>(IDictionary<string, object> arguments = null);

        /// <summary>Determines whether it is possible to resolve the instance of type <paramref name="type" />.</summary>
        /// <param name="type">Type of instance to resolve.</param>
        /// <param name="arguments">Optional constructor arguments.</param>
        /// <returns><b>true</b> if it is possible to resolve the instance; otherwise <b>false</b>.</returns>
        bool CanResolve(Type type, IDictionary<string, object> arguments = null);

        /// <summary>Resolves the component.</summary>
        /// <typeparam name="T">Type of the component to be resolved.</typeparam>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <typeparamref name="T"/>.</returns>
        T Resolve<T>(IDictionary<string, object> arguments = null);

        /// <summary>Resolves the component.</summary>
        /// <param name="type">Type of the component to be resolved.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <paramref name="type"/>.</returns>
        object Resolve(Type type, IDictionary<string, object> arguments = null);

        /// <summary>Resolves the component.</summary>
        /// <typeparam name="T">Type of the component to be resolved.</typeparam>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <typeparamref name="T"/>.</returns>
        T Resolve<T>(string name, IDictionary<string, object> arguments = null);

        /// <summary>Resolves the component.</summary>
        /// <param name="type">Type of the component to be resolved.</param>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <paramref name="type"/>.</returns>
        object Resolve(Type type, string name, IDictionary<string, object> arguments = null);

        /// <summary>Resolves components based on given type.</summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Enumeration of instances of types registered.</returns>
        IEnumerable<T> ResolveAll<T>(IDictionary<string, object> arguments = null);

        /// <summary>Resolves components based on given type.</summary>
        /// <param name="type">Type of the components.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Enumeration of instances of types registered.</returns>
        IEnumerable<object> ResolveAll(Type type, IDictionary<string, object> arguments = null);

        /// <summary>Resolves type of the component.</summary>
        /// <remarks>This method should return only resolvable, generic closed types.</remarks>
        /// <typeparam name="T">Type of the component to be resolved.</typeparam>
        /// <returns>Type of the implementation.</returns>
        Type ResolveType<T>();

        /// <summary>Resolves type of components based on given type.</summary>
        /// <remarks>This method should return only resolvable, generic closed types.</remarks>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <returns>Enumeration of types registered.</returns>
        IEnumerable<Type> ResolveAllTypes<T>();

        /// <summary>Resolves type of the component.</summary>
        /// <remarks>This method should return only resolvable, generic closed types.</remarks>
        /// <param name="serviceType">Type of the component to be resolved.</param>
        /// <returns>Type of the implementation.</returns>
        Type ResolveType(Type serviceType);

        /// <summary>Resolves type of components based on given type.</summary>
        /// <remarks>This method should return only resolvable, generic closed types.</remarks>
        /// <param name="serviceType">Type of the components.</param>
        /// <returns>Enumeration of types registered.</returns>
        IEnumerable<Type> ResolveAllTypes(Type serviceType);
    }
}