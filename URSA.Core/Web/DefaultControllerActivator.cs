using System;
using System.Linq;
using URSA.ComponentModel;

namespace URSA.Web
{
    /// <summary>Default implementation of the <see cref="IControllerActivator" />.</summary>
    public class DefaultControllerActivator : IControllerActivator
    {
        private IComponentProvider _container;

        /// <summary>Initializes a new instance of the <see cref="DefaultControllerActivator" /> class.</summary>
        /// <param name="container">Dependency injection container.</param>
        public DefaultControllerActivator(IComponentProvider container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            _container = container;
        }

        /// <inheritdoc />
        public IController CreateInstance(Type type)
        {
            Type candidate = type;
            if (_container.CanResolve(candidate))
            {
                return (IController)_container.Resolve(candidate);
            }
            else if ((type.IsGenericType) && (_container.CanResolve(candidate = type.GetGenericTypeDefinition())))
            {
                return (IController)_container.Resolve(type);
            }
            else
            {
                candidate = type.GetInterfaces()
                    .Where(@interface => typeof(IController).IsAssignableFrom(@interface))
                    .OrderByDescending(@interface => @interface.GetGenericArguments().Length)
                    .First();
                return (IController)_container.Resolve(candidate);
            }
        }
    }
}