using System;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Represents an <see cref="IOperation" /> and it's corresponding <see cref="IIriTemplate" /> if any.</summary>
    public struct LinkedOperation
    {
        private readonly IOperation _operation;
        private readonly IIriTemplate _iriTemplate;

        /// <summary>Initializes a new instance of the <see cref="LinkedOperation"/> struct.</summary>
        /// <param name="operation">The operation.</param>
        /// <param name="iriTemplate">The IRI template.</param>
        public LinkedOperation(IOperation operation, IIriTemplate iriTemplate)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            _operation = operation;
            _iriTemplate = iriTemplate;
        }

        /// <summary>Gets the operation.</summary>
        public IOperation Operation { get { return _operation; } }

        /// <summary>Gets the IRI template of the associated <see cref="LinkedOperation.Operation" />.</summary>
        public IIriTemplate IriTemplate { get { return _iriTemplate; } }
    }
}