using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
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

            var typeInfo = type.GetTypeInfo();
            if ((typeInfo.IsGenericType) && (_container.CanResolve(candidate = type.GetGenericTypeDefinition(), arguments)))
            {
                return (IController)_container.Resolve(candidate, arguments);
            }

            candidate = typeInfo.ImplementedInterfaces
                .Where(@interface => typeof(IController).IsAssignableFrom(@interface))
                .OrderByDescending(@interface => @interface.GetGenericArguments().Length)
                .First();
            return (IController)_container.Resolve(candidate, arguments);
        }
    }
}