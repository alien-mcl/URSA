using System.Collections.Generic;
using URSA.Web.Description;
using URSA.Web.Http;

namespace URSA.Web
{
    /// <summary>Defines possible argument value sources.</summary>
    public enum ArgumentValueSources
    {
        /// <summary>Defines a neutral value generated for an argument that was not bound at all.</summary>
        Neutral,

        /// <summary>Defines a default value provided by the underlying method's declaration.</summary>
        Default,

        /// <summary>Defines a bound value from the request.</summary>
        Bound
    }

    /// <summary>Defines a basic contract for request mapping.</summary>
    public interface IRequestMapping
    {
        /// <summary>Gets the target of the invocation.</summary>
        IController Target { get; }

        /// <summary>Gets the operation to be invoked.</summary>
        OperationInfo Operation { get; }

        /// <summary>Gets the mapped method route.</summary>
        Url MethodRoute { get; }

        /// <summary>Gets a map of argument indices and their corresponding source determining whether the value is a default, neutral or actually requested one.</summary>
        IDictionary<int, ArgumentValueSources> ArgumentSources { get; }

        /// <summary>Invokes the mapped method.</summary>
        /// <param name="arguments">Arguments for the invoked method.</param>
        /// <returns>Result of the invocation.</returns>
        object Invoke(params object[] arguments);
    }
}