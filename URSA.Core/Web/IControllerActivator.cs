using System;
using System.Collections.Generic;

namespace URSA.Web
{
    /// <summary>Provides a basic description of controller creation facility.</summary>
    public interface IControllerActivator
    {
        /// <summary>Creates an instance of the controller of type <paramref name="type" />.</summary>
        /// <param name="type">Type of the controller to create.</param>
        /// <param name="arguments">Optional constructor arguments.</param>
        /// <returns>Instance of the controller.</returns>
        IController CreateInstance(Type type, IDictionary<string, object> arguments = null);
    }
}