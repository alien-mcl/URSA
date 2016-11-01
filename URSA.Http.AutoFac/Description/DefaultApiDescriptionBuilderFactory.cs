using System;
using URSA.Web;
using URSA.Web.Http.Description;

namespace URSA.Http.AutoFac.Description
{
    /// <summary>Provides a basic implementation of the <see cref="IApiDescriptionBuilderFactory" /> facility.</summary>
    public class DefaultApiDescriptionBuilderFactory : IApiDescriptionBuilderFactory
    {
        private readonly Func<Type, IApiDescriptionBuilder> _factoryDelegate;

        /// <summary>Initializes a new instance of the <see cref="DefaultApiDescriptionBuilderFactory" /> class.</summary>
        /// <param name="factoryDelegate">Factory method to be used to create instances of the <see cref="IApiDescriptionBuilder" />.</param>
        public DefaultApiDescriptionBuilderFactory(Func<Type, IApiDescriptionBuilder> factoryDelegate)
        {
            _factoryDelegate = factoryDelegate;
        }

        /// <inheritdoc />
        public IApiDescriptionBuilder<T> Create<T>() where T : IController
        {
            return (IApiDescriptionBuilder<T>)Create(typeof(T));
        }

        /// <inheritdoc />
        public IApiDescriptionBuilder Create(Type type)
        {
            return _factoryDelegate(type);
        }
    }
}
