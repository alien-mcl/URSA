using System;
using URSA.Web.Description;

namespace URSA.Web
{
    /// <summary>Defines a basic contract for request mapping.</summary>
    public interface IRequestMapping
    {
        /// <summary>Gets the target of the invocation.</summary>
        IController Target { get; }

        /// <summary>Gets the operation to be invoked.</summary>
        OperationInfo Operation { get; }

        /// <summary>Gets the mapped method route.</summary>
        Uri MethodRoute { get; }

        /// <summary>Invokes the mapped method.</summary>
        /// <param name="arguments">Arguments for the invoked method.</param>
        /// <returns>Result of the invocation.</returns>
        object Invoke(params object[] arguments);
    }
}