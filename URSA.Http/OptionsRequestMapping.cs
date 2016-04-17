using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
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
        /// <param name="responseStatusCode">Designated HTTP response status code.</param>
        /// <param name="allowed">Allowed HTTP requests.</param>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        public OptionsRequestMapping(OperationInfo<Verb> operation, Uri methodRoute, HttpStatusCode responseStatusCode, params string[] allowed)
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

            Target = new OptionsController(responseStatusCode, allowed);
            Operation = operation;
            MethodRoute = methodRoute;
        }
        
        /// <inheritdoc />
        public IController Target { get; private set; }

        /// <inheritdoc />
        public Uri MethodRoute { get; private set; }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
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