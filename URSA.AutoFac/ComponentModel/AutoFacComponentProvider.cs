using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using URSA.AutoFac.ComponentModel;
using IContainer = Autofac.IContainer;

namespace URSA.ComponentModel
{
    /// <summary>Provides an <![CDATA[AutoFac]]> based implementation of the <see cref="IServiceProvider"/> interface.</summary>
    public class AutoFacComponentProvider : AutoFacComponentResolver, IComponentProvider
    {
        private readonly ILifetimeScope _container;

        /// <summary>Initializes a new instance of the <see cref="AutoFacComponentProvider"/> class.</summary>
        public AutoFacComponentProvider() : base(null, null)
        {
            var builder = new ContainerBuilder();
            //// TODO: Consider removing container registration.
            builder.Register(context => this).As<IComponentProvider>().InstancePerLifetimeScope();
            ((IComponentContextProvider)this).Container = _container = builder.Build();
        }

        private AutoFacComponentProvider(IComponentContextProvider parent, ILifetimeScope container) : base(parent, container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public IComponentProvider BeginNewScope()
        {
            return new AutoFacComponentProvider(this, _container.BeginLifetimeScope());
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
                var registration = builder.Register(context => factoryMethod(new AutoFacComponentResolver(this, context.Resolve<IComponentContext>()))).As(serviceType);
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
                var registration = builder.Register(context => factoryMethod(new AutoFacComponentResolver(this, context.Resolve<IComponentContext>()))).As(serviceType);
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
                case Lifestyles.Scoped:
                    registration.InstancePerLifetimeScope();
                    break;
            }

            UpdateContainerUsing(builder);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        /// <summary>Disposes this instance.</summary>
        /// <param name="disposing">Value indicating whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_container != null)
            {
                _container.Dispose();
            }
        }

        private void UpdateContainerUsing(ContainerBuilder builder)
        {
            var container = _container as IContainer;
            if (container != null)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                //// TODO: ContainerBuilder.Update is marked Obsolete!
                builder.Update(container);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
