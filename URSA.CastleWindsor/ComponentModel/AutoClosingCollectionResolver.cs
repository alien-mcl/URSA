using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;

namespace URSA.CastleWindsor.ComponentModel
{
    /// <summary>Provides an implementation of auto-closing collection resolution.</summary>
    public class AutoClosingCollectionResolver : ISubDependencyResolver
    {
        private readonly IKernel _kernel;

        /// <summary>Initializes a new instance of the <see cref="AutoClosingCollectionResolver"/> class.</summary>
        /// <param name="kernel">The kernel.</param>
        public AutoClosingCollectionResolver(IKernel kernel)
        {
            _kernel = kernel;
        }

        /// <inheritdoc />
        public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, Castle.Core.ComponentModel model, DependencyModel dependency)
        {
            Type genericArgument = null;
            if ((dependency.TargetType.IsGenericType) && (dependency.TargetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                genericArgument = dependency.TargetType.GetGenericArguments()[0];
            }
            else
            {
                dependency.TargetType.GetInterfaces().First(implemented => (implemented.IsGenericType) && (implemented.GetGenericTypeDefinition() == typeof(IEnumerable<>)) &&
                    ((genericArgument = implemented.GetGenericArguments()[0]) != null));
            }

            var handlers = _kernel.GetAssignableHandlers(genericArgument).Distinct(HandlerEqualityComparer.Instance);
            var components = handlers
                .Where(h => h.CurrentState == HandlerState.Valid)
                .Select(h => h.Resolve(new CreationContext(genericArgument, context, true)))
                .ToArray();
            var result = Array.CreateInstance(genericArgument, components.Length);
            components.CopyTo(result, 0);
            return result;
        }

        /// <inheritdoc />
        public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, Castle.Core.ComponentModel model, DependencyModel dependency)
        {
            if (dependency.TargetType == null)
            {
                return false;
            }

            Type genericArgument = null;
            if ((dependency.TargetType.IsGenericType) && (dependency.TargetType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                genericArgument = dependency.TargetType.GetGenericArguments()[0];
            }
            else
            {
                dependency.TargetType.GetInterfaces().Any(implemented => (implemented.IsGenericType) && (implemented.GetGenericTypeDefinition() == typeof(IEnumerable<>)) &&
                    ((genericArgument = implemented.GetGenericArguments()[0]) != null));
            }

            if ((genericArgument == null) || (!_kernel.HasComponent(genericArgument)))
            {
                return false;
            }

            return true;
        }
    }
}