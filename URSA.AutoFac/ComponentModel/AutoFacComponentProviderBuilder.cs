using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

namespace URSA.ComponentModel
{
    /// <summary>Provides an <![CDATA[AutoFac]]> based implementation of the <see cref="IServiceProvider"/> interface.</summary>
    public class AutoFacComponentProviderBuilder : IComponentProviderBuilder
    {
        private readonly ContainerBuilder _builder;

        /// <summary>Initializes a new instance of the <see cref="AutoFacComponentProviderBuilder"/> class.</summary>
        /// <param name="builder">Container builder.</param>
        internal AutoFacComponentProviderBuilder(ContainerBuilder builder)
        {
            _builder = builder;
        }

        /// <inheritdoc />
        public bool IsRoot { get { return false; } }

        /// <inheritdoc />
        public void Install(IEnumerable<Assembly> assemblies)
        {
            _builder.RegisterAssemblyModules(assemblies.ToArray());
        }

        /// <inheritdoc />
        public void Register<T, I>(string name, Func<T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient) where T : class where I : T
        {
            Register(typeof(T), typeof(I), name, factoryMethod, lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, string name, Func<object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
        {
            if (factoryMethod != null)
            {
                var registration = _builder.Register(context => factoryMethod()).As(serviceType);
                if (name != null)
                {
                    registration.Named(name, serviceType);
                }
            }
            else
            {
                var registration = _builder.RegisterType(implementationType).As(serviceType);
                if (name != null)
                {
                    registration.Named(name, serviceType);
                }
            }
        }

        /// <inheritdoc />
        public void Register<T, I>(Func<T> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient) where T : class where I : T
        {
            Register(typeof(T), typeof(I), factoryMethod, lifestyle);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, Type implementationType, Func<object> factoryMethod = null, Lifestyles lifestyle = Lifestyles.Transient)
        {
            if (factoryMethod != null)
            {
                var registration = _builder.Register(context => factoryMethod()).As(serviceType);
                switch (lifestyle)
                {
                    case Lifestyles.Singleton:
                        registration.SingleInstance();
                        break;
                    case Lifestyles.Transient:
                        registration.InstancePerDependency();
                        break;
                }
            }
            else
            {
                var registration = _builder.RegisterType(implementationType).As(serviceType);
                switch (lifestyle)
                {
                    case Lifestyles.Singleton:
                        registration.SingleInstance();
                        break;
                    case Lifestyles.Transient:
                        registration.InstancePerDependency();
                        break;
                }
            }
        }

        /// <inheritdoc />
        public void Register<T>(T instance) where T : class
        {
            Register(typeof(T), instance);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, object instance)
        {
            _builder.RegisterInstance(instance).As(serviceType);
        }

        /// <inheritdoc />
        public void Register<T>(string name, T instance) where T : class
        {
            Register(typeof(T), name, instance);
        }

        /// <inheritdoc />
        public void Register(Type serviceType, string name, object instance)
        {
            var registration = _builder.RegisterInstance(instance).As(serviceType);
            if (name != null)
            {
                registration.Named(name, serviceType);
            }
        }

        /// <inheritdoc />
        public void RegisterAll<T>(IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient) where T : class
        {
            RegisterAll(typeof(T), assemblies, lifestyle);
        }

        /// <inheritdoc />
        public void RegisterAll(Type serviceType, IEnumerable<Assembly> assemblies, Lifestyles lifestyle = Lifestyles.Transient)
        {
            var registration = _builder.RegisterAssemblyTypes(assemblies.ToArray()).AsSelf().As(serviceType);
            switch (lifestyle)
            {
                case Lifestyles.Singleton:
                    registration.SingleInstance();
                    break;
                case Lifestyles.Transient:
                    registration.InstancePerDependency();
                    break;
            }
        }
    }
}
