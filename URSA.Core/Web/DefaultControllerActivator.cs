using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.ComponentModel;

namespace URSA.Web
{
    /// <summary>Default implementation of the <see cref="IControllerActivator" />.</summary>
    public class DefaultControllerActivator : IControllerActivator
    {
        private readonly IComponentProvider _container;

        /// <summary>Initializes a new instance of the <see cref="DefaultControllerActivator" /> class.</summary>
        /// <param name="container">Dependency injection container.</param>
        [ExcludeFromCodeCoverage]
        public DefaultControllerActivator(IComponentProvider container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            _container = container;
        }

        /// <inheritdoc />
        public IController CreateInstance(Type type, IDictionary<string, object> arguments = null)
        {
            Type candidate = type;
            if (_container.CanResolve(candidate, arguments))
            {
                return (IController)_container.Resolve(candidate, arguments);
            }

            if ((type.IsGenericType) && (_container.CanResolve(candidate = type.GetGenericTypeDefinition(), arguments)))
            {
                return (IController)_container.Resolve(candidate, arguments);
            }

            candidate = type.GetInterfaces()
                .Where(@interface => typeof(IController).IsAssignableFrom(@interface))
                .OrderByDescending(@interface => @interface.GetGenericArguments().Length)
                .First();
            return (IController)_container.Resolve(candidate, arguments);
        }
    }
}