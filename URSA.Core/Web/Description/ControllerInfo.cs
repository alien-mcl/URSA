using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Description
{
    /// <summary>Describes a controller.</summary>
    [DebuggerDisplay("{Uri}", Name = "{Uri}")]
    public abstract class ControllerInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ControllerInfo" /> class.</summary>
        /// <param name="uri">Base uri of the controller.</param>
        /// <param name="operations">Operation details.</param>
        public ControllerInfo(Uri uri, params OperationInfo[] operations)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsAbsoluteUri)
            {
                throw new ArgumentNullException("uri");
            }

            Uri = uri;
            Operations = (operations != null ? operations : new OperationInfo[0]);
        }

        /// <summary>Gets the base uri of the controller.</summary>
        public Uri Uri { get; private set; }

        /// <summary>Gets the operation descriptors.</summary>
        public IEnumerable<OperationInfo> Operations { get; private set; }
    }

    /// <summary>Describes a controller.</summary>
    /// <typeparam name="T">Type of the controller.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Generic and non-generic interfaces. Suppression is OK.")]
    public class ControllerInfo<T> : ControllerInfo where T : IController
    {
        /// <summary>Initializes a new instance of the <see cref="ControllerInfo{T}" /> class.</summary>
        /// <param name="uri">Base uri of the controller.</param>
        /// <param name="operations">Operation details.</param>
        public ControllerInfo(Uri uri, params OperationInfo[] operations) : base(uri, operations)
        {
        }
    }
}