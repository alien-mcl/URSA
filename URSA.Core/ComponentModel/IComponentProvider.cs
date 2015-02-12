using System;
using System.Collections.Generic;
using System.Reflection;

namespace URSA.ComponentModel
{
    /// <summary>Defines component lifestyles.</summary>
    public enum Lifestyles
    {
        /// <summary>Singleton lifestyle.</summary>
        Singleton,

        /// <summary>Transient lifestyle.</summary>
        Transient
    }

    /// <summary>Provides a basic service provider facility interface.</summary>
    public interface IComponentProvider
    {
        /// <summary>Installs the component provider.</summary>
        /// <param name="assemblies">Assemblies to look for installers.</param>
        void Install(IEnumerable<Assembly> assemblies);

        /// <summary>Registers a component <typeparamref name="T"/> implemented by <typeparamref name="I"/>.</summary>
        /// <typeparam name="T">Type of the component to be registered.</typeparam>
        /// <typeparam name="I">Type implementing the component.</typeparam>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register<T, I>(string name, Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T;

        /// <summary>Registers a component <paramref name="serviceType"/> implemented by <paramref name="implementationType"/>.</summary>
        /// <param name="serviceType">Type of the component to be registered.</param>
        /// <param name="implementationType">Type implementing the component.</param>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register(Type serviceType, Type implementationType, string name, Lifestyles lifestyle = Lifestyles.Transient);

        /// <summary>Registers an instance of the component implementing <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="instance">Instance to be registered.</param>
        void Register<T>(string name, T instance) where T : class;

        /// <summary>Registers an instance of the component implementing <paramref name="serviceType" />.</summary>
        /// <param name="serviceType">Type of the component.</param>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="instance">Instance to be registered.</param>
        void Register(Type serviceType, string name, object instance);

        /// <summary>Registers a component <typeparamref name="T"/> implemented by <typeparamref name="I"/>.</summary>
        /// <typeparam name="T">Type of the component to be registered.</typeparam>
        /// <typeparam name="I">Type implementing the component.</typeparam>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register<T, I>(Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T;

        /// <summary>Registers a component <paramref name="serviceType"/> implemented by <paramref name="implementationType"/>.</summary>
        /// <param name="serviceType">Type of the component to be registered.</param>
        /// <param name="implementationType">Type implementing the component.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register(Type serviceType, Type implementationType, Lifestyles lifestyle = Lifestyles.Transient);

        /// <summary>Registers an instance of the component implementing <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <param name="instance">Instance to be registered.</param>
        void Register<T>(T instance) where T : class;

        /// <summary>Registers an instance of the component implementing <paramref name="serviceType" />.</summary>
        /// <param name="serviceType">Type of the component.</param>
        /// <param name="instance">Instance to be registered.</param>
        void Register(Type serviceType, object instance);

        /// <summary>Registers all components <typeparamref name="T"/>  found in given assemblies.</summary>
        /// <typeparam name="T">Type of the component to be registered.</typeparam>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void RegisterAll<T>(IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient) where T : class;

        /// <summary>Registers all components <paramref name="serviceType"/> found in given assemblies.</summary>
        /// <param name="serviceType">Type of the component to be registered.</param>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void RegisterAll(Type serviceType, IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient);

        /// <summary>Determines whether it is possible to resolve the instance of type <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Type of instance to resolve.</typeparam>
        /// <returns><b>true</b> if it is possible to resolve the instance; otherwise <b>false</b>.</returns>
        bool CanResolve<T>();

        /// <summary>Determines whether it is possible to resolve the instance of type <paramref name="T" />.</summary>
        /// <param name="type">Type of instance to resolve.</param>
        /// <returns><b>true</b> if it is possible to resolve the instance; otherwise <b>false</b>.</returns>
        bool CanResolve(Type type);

        /// <summary>Resolves the component.</summary>
        /// <typeparam name="T">Type of the component to be resolved.</typeparam>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <typeparamref name="T"/>.</returns>
        T Resolve<T>(params object[] arguments);

        /// <summary>Resolves the component.</summary>
        /// <param name="type">Type of the component to be resolved.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <paramref name="type"/>.</returns>
        object Resolve(Type type, params object[] arguments);

        /// <summary>Resolves the component.</summary>
        /// <typeparam name="T">Type of the component to be resolved.</typeparam>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <typeparamref name="T"/>.</returns>
        T Resolve<T>(string name, params object[] arguments);

        /// <summary>Resolves the component.</summary>
        /// <param name="type">Type of the component to be resolved.</param>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Implementation of the given <paramref name="type"/>.</returns>
        object Resolve(Type type, string name, params object[] arguments);

        /// <summary>Resolves components based on given type.</summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Enumeration of instances of types registered.</returns>
        IEnumerable<T> ResolveAll<T>(params object[] arguments);

        /// <summary>Resolves components based on given type.</summary>
        /// <param name="type">Type of the components.</param>
        /// <param name="arguments">Optional arguments to be passed for the resolved instance.</param>
        /// <returns>Enumeration of instances of types registered.</returns>
        IEnumerable<object> ResolveAll(Type type, params object[] arguments);

        /// <summary>Resolves type of the component.</summary>
        /// <typeparam name="T">Type of the component to be resolved.</typeparam>
        /// <returns>Type of the implementation.</returns>
        Type ResolveType<T>();

        /// <summary>Resolves type of components based on given type.</summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <returns>Enumeration of types registered.</returns>
        IEnumerable<Type> ResolveAllTypes<T>();

        /// <summary>Resolves type of the component.</summary>
        /// <param name="serviceType">Type of the component to be resolved.</param>
        /// <returns>Type of the implementation.</returns>
        Type ResolveType(Type serviceType);

        /// <summary>Resolves type of components based on given type.</summary>
        /// <param name="serviceType">Type of the components.</param>
        /// <returns>Enumeration of types registered.</returns>
        IEnumerable<Type> ResolveAllTypes(Type serviceType);
    }
}