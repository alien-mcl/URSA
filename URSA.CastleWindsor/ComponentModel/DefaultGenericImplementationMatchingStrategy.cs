using Castle.MicroKernel.Context;
using Castle.MicroKernel.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URSA.ComponentModel;

namespace URSA.CastleWindsor.ComponentModel
{
    internal class DefaultGenericImplementationMatchingStrategy : IGenericImplementationMatchingStrategy 
    {
        private WindsorComponentProvider _container;

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
            else if ((arguments = model.Implementation.GetGenericArguments()).Length > 0)
            {
                return arguments.SelectMany(argument => 
                    argument.GetGenericParameterConstraints().SelectMany(constrain => 
                        _container.ResolveAllTypes(constrain))).ToArray();
            }
            else
            {
                return new Type[] { typeof(object) };
            }
        }
    }
}