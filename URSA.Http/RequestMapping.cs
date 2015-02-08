using System;
using URSA.Web.Description.Http;

namespace URSA.Web.Http
{
    /// <summary>Describes a request mapping.</summary>
    public class RequestMapping : IRequestMapping
    {
        /// <summary>Initializes a new instance of the <see cref="RequestMapping" /> class.</summary>
        /// <param name="target">Target of the invocation.</param>
        /// <param name="operation">Operation to be invoked.</param>
        /// <param name="methodRoute">Method route.</param>
        public RequestMapping(IController target, OperationInfo operation, Uri methodRoute)
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
        public Uri MethodRoute { get; private set; }

        /// <inheritdoc />
        public OperationInfo Operation { get; private set; }

        URSA.Web.Description.OperationInfo IRequestMapping.Operation { get { return Operation; } }

        /// <inheritdoc />
        public object Invoke(params object[] arguments)
        {
            return Operation.UnderlyingMethod.Invoke(Target, arguments);
        }
    }
}