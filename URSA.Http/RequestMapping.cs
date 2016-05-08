using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Description;

namespace URSA.Web.Http
{
    /// <summary>Describes a request mapping.</summary>
    public class RequestMapping : IRequestMapping
    {
        /// <summary>Initializes a new instance of the <see cref="RequestMapping" /> class.</summary>
        /// <param name="target">Target of the invocation.</param>
        /// <param name="operation">Operation to be invoked.</param>
        /// <param name="methodRoute">Method route.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public RequestMapping(IController target, OperationInfo<Verb> operation, HttpUrl methodRoute)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            Target = target;
            Operation = operation;
            MethodRoute = methodRoute;
        }
        
        /// <inheritdoc />
        public IController Target { get; private set; }

        /// <inheritdoc />
        public HttpUrl MethodRoute { get; private set; }

        /// <summary>Gets the mapped method route.</summary>
        Url IRequestMapping.MethodRoute { get { return MethodRoute; } }

        /// <inheritdoc />
        OperationInfo IRequestMapping.Operation { get { return Operation; } }

        /// <summary>Gets the operation to be invoked.</summary>
        public OperationInfo<Verb> Operation { get; private set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public object Invoke(params object[] arguments)
        {
            return Operation.UnderlyingMethod.Invoke(Target, arguments);
        }
    }
}