using System;
using System.Linq.Expressions;

namespace URSA.Web
{
    /// <summary>Defines a basic hypermedia handling facility used by controllers.</summary>
    public interface IHypermediaFacility
    {
        /// <summary>Instructs the framework to inject that a given <paramref name="operation" /> should be injected into the payload</summary>
        /// <typeparam name="TController">Type of the controller owning this facility.</typeparam>
        /// <param name="operation">The operation description to be injected.</param>
        void Inject<TController>(Expression<Action<TController>> operation) where TController : IController;
    }
}
