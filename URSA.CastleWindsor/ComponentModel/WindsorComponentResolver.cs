using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using URSA.CastleWindsor.ComponentModel;

namespace URSA.ComponentModel
{
    /// <summary>Provides a Castle Windsor based implementation of the <see cref="IComponentResolver"/> interface.</summary>
    public sealed class WindsorComponentResolver : IComponentResolver
    {
        private readonly IKernel _kernel;

        internal WindsorComponentResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <inheritdoc />
        public bool IsRoot { get { return false; } }

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
            foreach (var handler in _kernel.GetAssignableHandlers(type))
            {
                bool canResolve = true;
                foreach (var dependency in handler.ComponentModel.Dependencies.OfType<ConstructorDependencyModel>())
                {
                    var creationContext = new CreationContext(handler, null, type, parameters, null, null);
                    if (_kernel.Resolver.CanResolve(creationContext, null, handler.ComponentModel, dependency))
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
            return (arguments != null ? _kernel.Resolve<T>(arguments.ToArguments()) : _kernel.Resolve<T>());
        }

        /// <inheritdoc />
        public object Resolve(Type type, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _kernel.Resolve(type, arguments.ToArguments()) : _kernel.Resolve(type));
        }

        /// <inheritdoc />
        public T Resolve<T>(string name, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _kernel.Resolve<T>(name, arguments.ToArguments()) : _kernel.Resolve<T>(name));
        }

        /// <inheritdoc />
        public object Resolve(Type type, string name, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _kernel.Resolve(name, type, arguments.ToArguments()) : _kernel.Resolve(name, type));
        }

        /// <inheritdoc />
        public IEnumerable<T> ResolveAll<T>(IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _kernel.ResolveAll<T>(arguments.ToArguments()) : _kernel.ResolveAll<T>());
        }

        /// <inheritdoc />
        public IEnumerable<object> ResolveAll(Type type, IDictionary<string, object> arguments = null)
        {
            return (arguments != null ? _kernel.ResolveAll(type, arguments.ToArguments()) : _kernel.ResolveAll(type)).Cast<object>();
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
            foreach (var handler in _kernel.GetAssignableHandlers(serviceType))
            {
                if ((handler.CurrentState == HandlerState.Valid) && (!handler.ComponentModel.Implementation.IsGenericTypeDefinition))
                {
                    result.Add(handler.ComponentModel.Implementation);
                }
                else if (handler.ComponentModel.Implementation.IsGenericTypeDefinition)
                {
                    var candidates = handler.ComponentModel.Implementation.GetGenericArguments().First()
                        .GetGenericParameterConstraints().SelectMany(constrain =>
                            _kernel.GetAssignableHandlers(constrain).Where(item => item.CurrentState == HandlerState.Valid).Select(item => item.ComponentModel.Implementation));
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
    }
}