using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.ComponentActivator;
using Castle.MicroKernel.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace URSA.CastleWindsor.ComponentModel
{
    /// <summary>Provides non-public constructors for Castle Windsor DI.</summary>
    public class NonPublicComponentActivator : DefaultComponentActivator
    {
        private readonly List<Type> _loadedTypes = new List<Type>();

        /// <summary>Initializes a new instance of the <see cref="NonPublicComponentActivator" /> class.</summary>
        /// <param name="model">Component model.</param>
        /// <param name="kernel">Castle Windsor kernel.</param>
        /// <param name="onCreation">Creation delegate.</param>
        /// <param name="onDestruction">Destruction delegate.</param>
        public NonPublicComponentActivator(Castle.Core.ComponentModel model, IKernelInternal kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
            : base(model, kernel, onCreation, onDestruction)
        {
        }

        /// <inheritdoc />
        protected override ConstructorCandidate SelectEligibleConstructor(CreationContext context)
        {
            lock (_loadedTypes)
            {
                if (!_loadedTypes.Contains(Model.Implementation))
                {
                    _loadedTypes.Add(Model.Implementation);
                    var ctors = Model.Implementation.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var ctor in ctors)
                    {
                        var parameters = ctor.GetParameters().Select(pi => new ConstructorDependencyModel(pi)).ToArray();
                        Model.AddConstructor(new ConstructorCandidate(ctor, parameters));
                    }
                }
            }

            return base.SelectEligibleConstructor(context);
        }
    }
}