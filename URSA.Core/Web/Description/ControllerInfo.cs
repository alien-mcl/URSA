using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Description
{
    /// <summary>Describes a controller.</summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Uri}", Name = "{Uri}")]
    public abstract class ControllerInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ControllerInfo" /> class.</summary>
        /// <param name="entryPoint">Entry point Uri prefix.</param>
        /// <param name="uri">Base uri of the controller including the <paramref name="entryPoint" /> prefix if any.</param>
        /// <param name="operations">Operation details.</param>
        protected ControllerInfo(Uri entryPoint, Uri uri, params OperationInfo[] operations)
        {
            if ((entryPoint != null) && (entryPoint.IsAbsoluteUri))
            {
                throw new ArgumentOutOfRangeException("entryPoint");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            EntryPoint = entryPoint;
            Uri = uri;
            Operations = (operations ?? new OperationInfo[0]);
            Arguments = new ConcurrentDictionary<string, object>();
        }

        /// <summary>Gets the base uri of the controller.</summary>
        public Uri Uri { get; private set; }

        /// <summary>Gets the operation descriptors.</summary>
        public IEnumerable<OperationInfo> Operations { get; private set; }

        /// <summary>Gets the entry point Uri prefix.</summary>
        public Uri EntryPoint { get; private set; }

        /// <summary>Gets the optional arguments to be used when creating a controller instance.</summary>
        public IDictionary<string, object> Arguments { get; private set; } 
    }

    /// <summary>Describes a controller.</summary>
    /// <typeparam name="T">Type of the controller.</typeparam>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Generic and non-generic interfaces. Suppression is OK.")]
    public class ControllerInfo<T> : ControllerInfo where T : IController
    {
        /// <summary>Initializes a new instance of the <see cref="ControllerInfo{T}" /> class.</summary>
        /// <param name="entryPoint">Entry point Uri prefix.</param>
        /// <param name="uri">Base uri of the controller including the <paramref name="entryPoint" /> prefix if any.</param>
        /// <param name="operations">Operation details.</param>
        public ControllerInfo(Uri entryPoint, Uri uri, params OperationInfo[] operations) : base(entryPoint, uri, operations)
        {
        }
    }
}