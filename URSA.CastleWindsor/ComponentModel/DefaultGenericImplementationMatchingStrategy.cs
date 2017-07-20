using System;
using System.Linq;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Handlers;
using URSA.ComponentModel;

namespace URSA.CastleWindsor.ComponentModel
{
    internal class DefaultGenericImplementationMatchingStrategy : IGenericImplementationMatchingStrategy
    {
        private readonly WindsorComponentProvider _container;

        internal DefaultGenericImplementationMatchingStrategy(WindsorComponentProvider container)
        {
            _container = container;
        }

        /// <inheritdoc />
        public Type[] GetGenericArguments(Castle.Core.ComponentModel model, CreationContext context)
        {
            Type[] arguments;
            if (context.RequestedType.GenericTypeArguments.Length > 0)
            {
                return context.RequestedType.GenericTypeArguments;
            }

            if ((arguments = model.Implementation.GetGenericArguments()).Length > 0)
            {
                return arguments.SelectMany(argument =>
                    argument.GetGenericParameterConstraints().SelectMany(constrain =>
                        _container.ResolveAllTypes(constrain))).ToArray();
            }

            return new Type[] { typeof(object) };
        }
    }
}