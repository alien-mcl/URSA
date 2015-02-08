﻿using Castle.MicroKernel;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using URSA.CastleWindsor.ComponentModel;
using URSA.ComponentModel;

namespace URSA.ComponentModel
{
    /// <summary>Provides a Castle Windsor based implementation of the <see cref="IServiceProvider"/> interface.</summary>
    public sealed class WindsorComponentProvider : IComponentProvider, IDisposable
    {
        private IWindsorContainer _container;
        private IGenericImplementationMatchingStrategy _genericImplementationMatchingStrategy;

        /// <summary>Initializes a new instance of the <see cref="WindsorComponentProvider"/> class.</summary>
        public WindsorComponentProvider()
        {
            _container = new WindsorContainer();
            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel, true));
            _container.Register(Component.For<WindsorComponentProvider>().Instance(this));
            _genericImplementationMatchingStrategy = new DefaultGenericImplementationMatchingStrategy(this);
        }

        internal IGenericImplementationMatchingStrategy GenericImplementationMatchingStrategy { get { return _genericImplementationMatchingStrategy; } }

        /// <inheritdoc />
        public void Install(IEnumerable<Assembly> assemblies)
        {
            assemblies.ForEach(assembly => _container.Install(FromAssembly.Instance(assembly)));
        }

        /// <inheritdoc />
        public void Register<T, I>(string name, Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T
        {
            Register(typeof(T), typeof(I), name, lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, string name, Lifestyles lifestyle = Lifestyles.Transient)
        {
            var registration = Component.For(serviceType).ImplementedBy(implementationType, _genericImplementationMatchingStrategy);
            switch (lifestyle)
            {
                case Lifestyles.Transient:
                    registration = registration.LifestyleTransient();
                    break;
                case Lifestyles.Singleton:
                    registration = registration.LifestyleSingleton();
                    break;
            }

            if (name != null)
            {
                registration = registration.Named(name);
            }

            _container.Register(registration);
        }

        /// <inheritdoc />
        public void Register<T>(string name, T instance) where T : class
        {
            _container.Register(Component.For<T>().Instance(instance).Named(name).LifestyleSingleton());
        }

        /// <inheritdoc />
        public void Register(Type serviceType, string name, object instance)
        {
            _container.Register(Component.For(serviceType).Instance(instance).Named(name).LifestyleSingleton());
        }

        /// <inheritdoc />
        public void Register<T, I>(Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T
        {
            Register(typeof(T), typeof(I), lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, Lifestyles lifestyle = Lifestyles.Transient)
        {
            Register(serviceType, implementationType, null, lifestyle);
        }

        /// <inheritdoc />
        public void Register<T>(T instance) where T : class
        {
            _container.Register(Component.For<T>().Instance(instance).LifestyleSingleton());
        }

        /// <inheritdoc />
        public void Register(Type serviceType, object instance)
        {
            _container.Register(Component.For(serviceType).Instance(instance).LifestyleSingleton());
        }

        /// <inheritdoc />
        public void RegisterAll<T>(IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient) where T : class
        {
            RegisterAll(typeof(T), assemblies, lifestyle);
        }

        /// <inheritdoc />
        public void RegisterAll(Type serviceType, IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient)
        {
            assemblies.ForEach(
                assembly =>
                {
                    var registration = Classes.FromAssembly(assembly).BasedOn(serviceType).WithService.FromInterface();
                    switch (lifestyle)
                    {
                        case Lifestyles.Singleton:
                            registration = registration.LifestyleSingleton();
                            break;
                        case Lifestyles.Transient:
                            registration = registration.LifestyleTransient();
                            break;
                    }

                    _container.Register(registration);
                });
        }

        /// <inheritdoc />
        public T Resolve<T>(params object[] arguments)
        {
            return _container.Resolve<T>(new Arguments(arguments));
        }

        /// <inheritdoc />
        public object Resolve(Type type, params object[] arguments)
        {
            return _container.Resolve(type, new Arguments(arguments));
        }

        /// <inheritdoc />
        public T Resolve<T>(string name, params object[] arguments)
        {
            return _container.Resolve<T>(name, new Arguments(arguments));
        }

        /// <inheritdoc />
        public object Resolve(Type type, string name, params object[] arguments)
        {
            return _container.Resolve(name, type, new Arguments(arguments));
        }

        /// <inheritdoc />
        public IEnumerable<T> ResolveAll<T>(params object[] arguments)
        {
            return _container.ResolveAll<T>(new Arguments(arguments));
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveAll(Type type, params object[] arguments)
        {
            return _container.ResolveAll(type, new Arguments(arguments)).Cast<object>();
        }

        /// <inheritdoc />
        public Type ResolveType(Type serviceType)
        {
            return ResolveAllTypes(serviceType).FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<Type> ResolveAllTypes(Type serviceType)
        {
            return _container.Kernel.GetAssignableHandlers(serviceType)
                .Where(handler => handler.CurrentState == HandlerState.Valid)
                .Select(handler => handler.ComponentModel.Implementation);
        }

        /// <inheritdoc />
        public Type ResolveType<T>()
        {
            return ResolveAllTypes<T>().FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<Type> ResolveAllTypes<T>()
        {
            return _container.Kernel.GetAssignableHandlers(typeof(T))
                .Where(handler => handler.CurrentState == HandlerState.Valid)
                .Select(handler => handler.ComponentModel.Implementation);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_container != null)
            {
                _container.Dispose();
            }
        }
    }
}