using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http;

namespace URSA.Web.Description
{
    /// <summary>Describes a controller.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    [DebuggerDisplay("{Url}", Name = "{Url}")]
    public abstract class ControllerInfo : SecurableResourceInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ControllerInfo" /> class.</summary>
        /// <param name="entryPoint">An entry point.</param>
        /// <param name="url">Base URL of the controller including the <paramref name="entryPoint" /> prefix if any.</param>
        /// <param name="operations">Operation details.</param>
        protected ControllerInfo(EntryPointInfo entryPoint, Url url, params OperationInfo[] operations) : base(url)
        {
            EntryPoint = entryPoint;
            foreach (var operation in Operations = (operations ?? new OperationInfo[0]))
            {
                operation.Controller = this;
            }

            Arguments = new ConcurrentDictionary<string, object>();
        }

        /// <summary>Gets the operation descriptors.</summary>
        public IEnumerable<OperationInfo> Operations { get; private set; }

        /// <summary>Gets the entry point URL prefix.</summary>
        public EntryPointInfo EntryPoint { get; private set; }

        /// <summary>Gets the optional arguments to be used when creating a controller instance.</summary>
        public IDictionary<string, object> Arguments { get; private set; }

        /// <summary>Gets the type of the controller.</summary>
        public abstract Type ControllerType { get; }

        /// <inheritdoc />
        public override SecurableResourceInfo Owner { get { return EntryPoint; } }
    }

    /// <summary>Describes a controller.</summary>
    /// <typeparam name="T">Type of the controller.</typeparam>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Generic and non-generic interfaces. Suppression is OK.")]
    public class ControllerInfo<T> : ControllerInfo where T : IController
    {
        /// <summary>Initializes a new instance of the <see cref="ControllerInfo{T}" /> class.</summary>
        /// <param name="entryPoint">An entry point.</param>
        /// <param name="url">Base URL of the controller including the <paramref name="entryPoint" /> prefix if any.</param>
        /// <param name="operations">Operation details.</param>
        public ControllerInfo(EntryPointInfo entryPoint, Url url, params OperationInfo[] operations) : base(entryPoint, url, operations)
        {
        }

        /// <inheritdoc />
        public override Type ControllerType { get { return typeof(T); } }
    }
}