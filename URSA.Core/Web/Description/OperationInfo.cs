using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace URSA.Web.Description
{
    /// <summary>Describes an controller operation.</summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Uri}", Name = "{Uri}")]
    public class OperationInfo
    {
        /// <summary>Initializes a new instance of the <see cref="OperationInfo" /> class.</summary>
        /// <param name="underlyingMethod">Actual underlying method.</param>
        /// <param name="uri">Base relative uri of the method without arguments.</param>
        /// <param name="uriTemplate">Relative uri template with all arguments included.</param>
        /// <param name="arguments">Argument descriptions.</param>
        public OperationInfo(MethodInfo underlyingMethod, Uri uri, string uriTemplate, params ArgumentInfo[] arguments)
        {
            if (underlyingMethod == null)
            {
                throw new ArgumentNullException("underlyingMethod");
            }

            if (!typeof(IController).IsAssignableFrom(underlyingMethod.DeclaringType))
            {
                throw new ArgumentOutOfRangeException("underlyingMethod");
            }

            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException("uri");
            }

            if (!String.IsNullOrEmpty(uriTemplate))
            {
                if (arguments == null)
                {
                    throw new ArgumentNullException("arguments");
                }

                if (arguments.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("arguments");
                }
            }

            UnderlyingMethod = underlyingMethod;
            Uri = uri;
            UriTemplate = uriTemplate;
            Arguments = (arguments != null ? arguments : new ArgumentInfo[0]);
            var parameters = UnderlyingMethod.GetParameters();
            foreach (var argument in Arguments)
            {
                if (!parameters.Contains(argument.Parameter))
                {
                    throw new ArgumentOutOfRangeException("arguments");
                }

                argument.Method = UnderlyingMethod;
            }
        }

        /// <summary>Gets the actual underlying method.</summary>
        public MethodInfo UnderlyingMethod { get; private set; }

        /// <summary>Gets the base uri of the method without arguments.</summary>
        public Uri Uri { get; private set; }

        /// <summary>Gets the uri template with all arguments included.</summary>
        public string UriTemplate { get; private set; }

        /// <summary>Gets the argument descriptions.</summary>
        public IEnumerable<ArgumentInfo> Arguments { get; private set; }
    }
}