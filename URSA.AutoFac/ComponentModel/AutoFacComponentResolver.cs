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
    /// <summary>Provides an <![CDATA[AutoFac]]> based implementation of the <see cref="IComponentResolver"/> interface.</summary>
    public class AutoFacComponentResolver : IComponentResolver, IComponentContextProvider
    {
        private readonly IComponentContextProvider _parent;
        private IComponentContext _container;

        internal AutoFacComponentResolver(IComponentContextProvider parent, IComponentContext container)
        {
            _parent = parent;
            _container = container;
        }

        /// <inheritdoc />
        public bool IsRoot { get { return _parent == null; } }

        IComponentContext IComponentContextProvider.Container { get { return _container; } set { _container = value; } }

        IComponentContextProvider IComponentContextProvider.Parent { get { return _parent; } }

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
                IComponentContextProvider current = this;
                do
                {
                    result = result.Concat(from registration in current.Container.ComponentRegistry.Registrations
                                           from service in registration.Services.OfType<IServiceWithType>()
                                           where service.ServiceType == type
                                           from ctor in registration.Activator.LimitType.GetTypeInfo().GetConstructors()
                                           let parameters = ctor.GetParameters()
                                           where arguments.Count(argument =>
                                               parameters.Any(parameter => (parameter.Name == argument.Key) && (parameter.ParameterType.GetTypeInfo().IsInstanceOfType(argument.Value)))) == arguments.Count
                                           select registration);
                    current = current.Parent;
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
    }
}
