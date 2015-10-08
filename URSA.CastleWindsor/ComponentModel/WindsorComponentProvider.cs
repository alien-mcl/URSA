using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Handlers;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.Core;
using URSA.CastleWindsor.ComponentModel;
using URSA.ComponentModel;

namespace URSA.ComponentModel
{
    /// <summary>Provides a Castle Windsor based implementation of the <see cref="IServiceProvider"/> interface.</summary>
    public sealed class WindsorComponentProvider : IComponentProvider, IDisposable
    {
        private readonly IWindsorContainer _container;
        private readonly IGenericImplementationMatchingStrategy _genericImplementationMatchingStrategy;

        /// <summary>Initializes a new instance of the <see cref="WindsorComponentProvider"/> class.</summary>
        public WindsorComponentProvider()
        {
            _container = new WindsorContainer();
            _container.Kernel.Resolver.AddSubResolver(new AutoClosingCollectionResolver(_container.Kernel));
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
        public void Register<T, I>(string name, Func<T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T
        {
            Register(typeof(T), typeof(I), name, factoryMethod, lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, string name, Func<object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
        {
            var registration = Component.For(serviceType).ImplementedBy(implementationType, _genericImplementationMatchingStrategy);
            if (factoryMethod != null)
            {
                registration = registration.UsingFactoryMethod(factoryMethod);
            }

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
        public void Register<T, I>(Func<T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
            where T : class
            where I : T
        {
            Register(typeof(T), typeof(I), factoryMethod, lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, Func<object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
        {
            Register(serviceType, implementationType, null, factoryMethod, lifestyle);
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
        public bool CanResolve<T>(IDictionary<string, object> arguments = null)
        {
            return CanResolve(typeof(T));
        }

        /// <inheritdoc />
        public bool CanResolve(Type type, IDictionary<string, object> arguments = null)
        {
            if ((arguments == null) || (arguments.Count == 0))
            {
                return ResolveAllTypes(type).Any();
            }

            var parameters = arguments.ToArguments();
            foreach (var handler in _container.Kernel.GetAssignableHandlers(type))
            {
                bool canResolve = true;
                foreach (var dependency in handler.ComponentModel.Dependencies.OfType<ConstructorDependencyModel>())
                {
                    var creationContext = new CreationContext(handler, null, type, parameters, null, null);
                    if (_container.Kernel.Resolver.CanResolve(creationContext, null, handler.ComponentModel, dependency))
                    {
                        continue;
                    }

                    canResolve = false;
                    break;
                }

                if (canResolve)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public T Resolve<T>(IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _container.Resolve<T>(arguments.ToArguments()) : _container.Resolve<T>());
        }

        /// <inheritdoc />
        public object Resolve(Type type, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _container.Resolve(type, arguments.ToArguments()) : _container.Resolve(type));
        }

        /// <inheritdoc />
        public T Resolve<T>(string name, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _container.Resolve<T>(name, arguments.ToArguments()) : _container.Resolve<T>(name));
        }

        /// <inheritdoc />
        public object Resolve(Type type, string name, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _container.Resolve(name, type, arguments.ToArguments()) : _container.Resolve(name, type));
        }

        /// <inheritdoc />
        public IEnumerable<T> ResolveAll<T>(IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _container.ResolveAll<T>(arguments.ToArguments()) : _container.ResolveAll<T>());
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveAll(Type type, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _container.ResolveAll(type, arguments.ToArguments()) : _container.ResolveAll(type)).Cast<object>();
        }

        /// <inheritdoc />
        public Type ResolveType(Type serviceType)
        {
            return ResolveAllTypes(serviceType).FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<Type> ResolveAllTypes(Type serviceType)
        {
            IList<Type> result = new List<Type>();
            foreach (var handler in _container.Kernel.GetAssignableHandlers(serviceType))
            {
                if ((handler.CurrentState == HandlerState.Valid) && (!handler.ComponentModel.Implementation.IsGenericTypeDefinition))
                {
                    result.Add(handler.ComponentModel.Implementation);
                }
                else if (handler.ComponentModel.Implementation.IsGenericTypeDefinition)
                {
                    var candidates = handler.ComponentModel.Implementation.GetGenericArguments().First()
                        .GetGenericParameterConstraints().SelectMany(constrain => 
                            _container.Kernel.GetAssignableHandlers(constrain).Where(item => item.CurrentState == HandlerState.Valid).Select(item => item.ComponentModel.Implementation));
                    foreach (var type in candidates)
                    {
                        if (type != handler.ComponentModel.Implementation)
                        {
                            result.Add(handler.ComponentModel.Implementation.MakeGenericType(type));
                        }
                    }
                }
            }

            return result;
        }

        /// <inheritdoc />
        public Type ResolveType<T>()
        {
            return ResolveAllTypes<T>().FirstOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<Type> ResolveAllTypes<T>()
        {
            return ResolveAllTypes(typeof(T));
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