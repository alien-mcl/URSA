using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Description;
using URSA.Web.Http.Description;

namespace URSA.Web.Http
{
    /// <summary>Describes an HTTP OPTIONS request mapping.</summary>
    public class OptionsRequestMapping : IRequestMapping
    {
        /// <summary>Initializes a new instance of the <see cref="OptionsRequestMapping" /> class.</summary>
        /// <param name="operation">Operation to be invoked.</param>
        /// <param name="methodRoute">Method route.</param>
        /// <param name="allowed">Allowed HTTP requests.</param>
        [ExcludeFromCodeCoverage]
        public OptionsRequestMapping(OperationInfo<Verb> operation, Uri methodRoute, params string[] allowed)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (allowed == null)
            {
                throw new ArgumentNullException("allowed");
            }

            if (allowed.Length == 0)
            {
                throw new ArgumentOutOfRangeException("allowed");
            }

            Target = new OptionsController(allowed);
            Operation = operation;
            MethodRoute = methodRoute;
        }
        
        /// <inheritdoc />
        public IController Target { get; private set; }

        /// <inheritdoc />
        public Uri MethodRoute { get; private set; }

        /// <inheritdoc />
        OperationInfo IRequestMapping.Operation { get { return Operation; } }

        /// <summary>Gets the operation to be invoked.</summary>
        public OperationInfo<Verb> Operation { get; private set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public object Invoke(params object[] arguments)
        {
            return Operation.UnderlyingMethod.Invoke(Target, arguments);
        }
    }
}