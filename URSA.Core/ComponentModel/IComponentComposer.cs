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

        /// <summary>Per existing scope lifestyle.</summary>
        Scoped,

        /// <summary>Transient lifestyle.</summary>
        Transient
    }

    /// <summary>Provides a basic service provider facility interface.</summary>
    public interface IComponentComposer
    {
        /// <summary>Gets a value indicating whether this is a root component provider.</summary>
        bool IsRoot { get; }

        /// <summary>Installs the component provider.</summary>
        /// <param name="assemblies">Assemblies to look for installers.</param>
        void Install(IEnumerable<Assembly> assemblies);

        /// <summary>Registers a component <typeparamref name="T"/> implemented by <typeparamref name="I"/>.</summary>
        /// <typeparam name="T">Type of the component to be registered.</typeparam>
        /// <typeparam name="I">Type implementing the component.</typeparam>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="factoryMethod">Optional factory method.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register<T, I>(string name, Func<IComponentResolver, T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T;

        /// <summary>Registers a component <paramref name="serviceType"/> implemented by <paramref name="implementationType"/>.</summary>
        /// <param name="serviceType">Type of the component to be registered.</param>
        /// <param name="implementationType">Type implementing the component.</param>
        /// <param name="name">Name of the implementation.</param>
        /// <param name="factoryMethod">Optional factory method.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register(Type serviceType, Type implementationType, string name, Func<IComponentResolver, object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient);

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
        /// <param name="factoryMethod">Optional factory method.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register<T, I>(Func<IComponentResolver, T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T;

        /// <summary>Registers a component <paramref name="serviceType"/> implemented by <paramref name="implementationType"/>.</summary>
        /// <param name="serviceType">Type of the component to be registered.</param>
        /// <param name="implementationType">Type implementing the component.</param>
        /// <param name="factoryMethod">Optional factory method.</param>
        /// <param name="lifestyle">Lifestyle of the registration.</param>
        void Register(Type serviceType, Type implementationType, Func<IComponentResolver, object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient);

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
    }
}