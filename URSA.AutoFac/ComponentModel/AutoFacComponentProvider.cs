using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using URSA.AutoFac.ComponentModel;
using IContainer = Autofac.IContainer;

namespace URSA.ComponentModel
{
    /// <summary>Provides an <![CDATA[AutoFac]]> based implementation of the <see cref="IServiceProvider"/> interface.</summary>
    public class AutoFacComponentProvider : IComponentProvider, IComponentContextProvider
    {
        private readonly AutoFacComponentProvider _parent;
        private readonly ILifetimeScope _container;

        /// <summary>Initializes a new instance of the <see cref="AutoFacComponentProvider"/> class.</summary>
        public AutoFacComponentProvider()
        {
            var builder = new ContainerBuilder();
            //// TODO: Consider removing container registration.
            builder.Register(context => this).As<IComponentProvider>().InstancePerLifetimeScope();
            _container = builder.Build();
            _parent = null;
        }

        private AutoFacComponentProvider(AutoFacComponentProvider parent, ILifetimeScope container)
        {
            _parent = parent;
            _container = container;
        }

        /// <inheritdoc />
        public bool IsRoot { get { return _parent == null; } }

        IComponentContext IComponentContextProvider.Container { get { return _container; } }

        IComponentContextProvider IComponentContextProvider.Parent { get { return _parent; } }

        /// <inheritdoc />
        public IComponentProvider BeginNewScope()
        {
            return new AutoFacComponentProvider(this, _container.BeginLifetimeScope(AutoFac.RegistrationExtensions.HttpRequestLifetimeScopeTag));
        }

        /// <inheritdoc />
        public void Install(IEnumerable<Assembly> assemblies)
        {
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(assemblies.ToArray());
            UpdateContainerUsing(builder);
        }

        /// <inheritdoc />
        public void Register<T, I>(string name, Func<IComponentResolver, T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient) where T : class where I : T
        {
            Register(typeof(T), typeof(I), name, factoryMethod, lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, string name, Func<IComponentResolver, object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
        {
            var builder = new ContainerBuilder();
            if (factoryMethod != null)
            {
                var registration = builder.Register(context => factoryMethod(new AutoFacScopedComponentResolver(this, context.Resolve<IComponentContext>()))).As(serviceType);
                if (name != null)
                {
                    registration.Named(name, serviceType);
                }

                switch (lifestyle)
                {
                    case Lifestyles.Singleton:
                        registration.SingleInstance();
                        break;
                    case Lifestyles.Transient:
                        registration.InstancePerDependency();
                        break;
                    case Lifestyles.Scoped:
                        registration.InstancePerLifetimeScope();
                        break;
                }
            }
            else
            {
                var registration = builder.RegisterType(implementationType).As(serviceType);
                if (name != null)
                {
                    registration = registration.Named(name, serviceType);
                }

                switch (lifestyle)
                {
                    case Lifestyles.Singleton:
                        registration.SingleInstance();
                        break;
                    case Lifestyles.Transient:
                        registration.InstancePerDependency();
                        break;
                    case Lifestyles.Scoped:
                        registration.InstancePerLifetimeScope();
                        break;
                }
            }

            UpdateContainerUsing(builder);
        }

        /// <inheritdoc />
        public void Register<T, I>(Func<IComponentResolver, T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient) where T : class where I : T
        {
            Register(typeof(T), typeof(I), factoryMethod, lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, Func<IComponentResolver, object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
        {
            var builder = new ContainerBuilder();
            if (factoryMethod != null)
            {
                var registration = builder.Register(context => factoryMethod(new AutoFacScopedComponentResolver(this, context.Resolve<IComponentContext>()))).As(serviceType);
                switch (lifestyle)
                {
                    case Lifestyles.Singleton:
                        registration.SingleInstance();
                        break;
                    case Lifestyles.Transient:
                        registration.InstancePerDependency();
                        break;
                    case Lifestyles.Scoped:
                        registration.InstancePerLifetimeScope();
                        break;
                }
            }
            else
            {
                var registration = builder.RegisterType(implementationType).As(serviceType);
                switch (lifestyle)
                {
                    case Lifestyles.Singleton:
                        registration.SingleInstance();
                        break;
                    case Lifestyles.Transient:
                        registration.InstancePerDependency();
                        break;
                    case Lifestyles.Scoped:
                        registration.InstancePerLifetimeScope();
                        break;
                }
            }

            UpdateContainerUsing(builder);
        }

        /// <inheritdoc />
        public void Register<T>(T instance) where T : class
        {
            Register(typeof(T), instance);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, object instance)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance).As(serviceType);
            UpdateContainerUsing(builder);
        }

        /// <inheritdoc />
        public void Register<T>(string name, T instance) where T : class
        {
            Register(typeof(T), name, instance);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, string name, object instance)
        {
            var builder = new ContainerBuilder();
            var registration = builder.RegisterInstance(instance).As(serviceType);
            if (name != null)
            {
                registration.Named(name, serviceType);
            }

            UpdateContainerUsing(builder);
        }

        /// <inheritdoc />
        public bool CanResolve<T>(IDictionary<string, object> arguments = null)
        {
            return CanResolve(typeof(T), arguments);
        }

        /// <inheritdoc />
        public bool CanResolve(Type type, IDictionary<string, object> arguments = null)
        {
            if (arguments != null)
            {
                var result = (IEnumerable<IComponentRegistration>)new IComponentRegistration[0];
                var current = this;
                do
                {
                    result = result.Concat(from registration in current._container.ComponentRegistry.Registrations
                                           from service in registration.Services.OfType<IServiceWithType>()
                                           where service.ServiceType == type
                                           from ctor in registration.Activator.LimitType.GetTypeInfo().GetConstructors()
                                           let parameters = ctor.GetParameters()
                                           where arguments.Count(argument =>
                                               parameters.Any(parameter => (parameter.Name == argument.Key) && (parameter.ParameterType.GetTypeInfo().IsInstanceOfType(argument.Value)))) == arguments.Count
                                           select registration);
                    current = current._parent;
                }
                while (current != null);
                return result.Any();
            }

            return _container.IsRegistered(type);
        }

        /// <inheritdoc />
        public T Resolve<T>(IDictionary<string, object> arguments = null)
        {
            return (T)Resolve(typeof(T), arguments);
        }

        /// <inheritdoc />
        public object Resolve(Type type, IDictionary<string, object> arguments = null)
        {
            return arguments != null ? _container.Resolve(type, arguments.Select(argument => new NamedParameter(argument.Key, argument.Value))) : _container.Resolve(type);
        }

        /// <inheritdoc />
        public T Resolve<T>(string name, IDictionary<string, object> arguments = null)
        {
            return (T)Resolve(typeof(T), name, arguments);
        }

        /// <inheritdoc />
        public object Resolve(Type type, string name, IDictionary<string, object> arguments = null)
        {
            return arguments != null ? _container.ResolveNamed(name, type, arguments.Select(argument => new NamedParameter(argument.Key, argument.Value))) : _container.ResolveNamed(name, type);
        }

        /// <inheritdoc />
        public IEnumerable<T> ResolveAll<T>(IDictionary<string, object> arguments = null)
        {
            return (IEnumerable<T>)ResolveAll(typeof(T), arguments);
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveAll(Type type, IDictionary<string, object> arguments = null)
        {
            type = type.MakeArrayType();
            return (IEnumerable<object>)(arguments != null ? _container.Resolve(type, arguments.Select(argument => new NamedParameter(argument.Key, argument.Value))) : _container.Resolve(type));
        }

        /// <inheritdoc />
        public Type ResolveType<T>()
        {
            return ResolveType(typeof(T));
        }

        /// <inheritdoc />
        public Type ResolveType(Type serviceType)
        {
            return ResolveAllTypes(serviceType).First();
        }

        /// <inheritdoc />
        public IEnumerable<Type> ResolveAllTypes<T>()
        {
            return ResolveAllTypes(typeof(T));
        }

        /// <inheritdoc />
        public IEnumerable<Type> ResolveAllTypes(Type serviceType)
        {
            return (from registration in _container.ComponentRegistry.Registrations
                    from service in registration.Services.OfType<IServiceWithType>()
                    where service.ServiceType == serviceType
                    let resultingType = registration.Activator.LimitType
                    let resultingTypes = (!resultingType.GetTypeInfo().IsGenericType || resultingType.IsConstructedGenericType) ? new Type[] { resultingType } : (
                        from someRegistration in _container.ComponentRegistry.Registrations
                        from someService in someRegistration.Services.OfType<IServiceWithType>()
                        where (!someRegistration.Activator.LimitType.GetTypeInfo().IsGenericType || someRegistration.Activator.LimitType.IsConstructedGenericType) &&
                            someService.ServiceType.IsAssignableFrom(resultingType.GetGenericArguments()[0].GetTypeInfo().GetGenericParameterConstraints()[0])
                        select resultingType.MakeGenericType(someRegistration.Activator.LimitType)).Distinct()
                    from actualType in resultingTypes
                    select actualType).Distinct();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_container != null)
            {
                _container.Dispose();
            }
        }

        /// <inheritdoc />
        public void Dispose<T>(T instance)
        {
            Dispose(typeof(T), instance);
        }

        /// <inheritdoc />
        public void Dispose(Type serviceType, object instance)
        {
            //// TODO: Manual instance disposal in AutoFac?
        }

        /// <inheritdoc />
        public void RegisterAll<T>(IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient) where T : class
        {
            RegisterAll(typeof(T), assemblies, lifestyle);
        }

        /// <inheritdoc />
        public void RegisterAll(Type serviceType, IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient)
        {
            var builder = new ContainerBuilder();
            var registration = builder.RegisterAssemblyTypes(assemblies.ToArray()).AsSelf().As(serviceType);
            switch (lifestyle)
            {
                case Lifestyles.Singleton:
                    registration.SingleInstance();
                    break;
                case Lifestyles.Transient:
                    registration.InstancePerDependency();
                    break;
            }

            UpdateContainerUsing(builder);
        }

        private void UpdateContainerUsing(ContainerBuilder builder)
        {
            var container = _container as IContainer;
            if (container != null)
            {
                builder.Update(container);
            }
        }
    }
}
