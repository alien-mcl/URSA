using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace URSA.Web.Description
{
    /// <summary>Describes an controller operation.</summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{ProtocolSpecificCommand} {UriTemplate}", Name = "{Uri}")]
    public abstract class OperationInfo
    {
        /// <summary>Initializes a new instance of the <see cref="OperationInfo" /> class.</summary>
        /// <param name="underlyingMethod">Actual underlying method.</param>
        /// <param name="uri">Base relative uri of the method without arguments.</param>
        /// <param name="uriTemplate">Relative uri template with all arguments included.</param>
        /// <param name="templateRegex">Regular expression template with all arguments included.</param>
        /// <param name="values">Values descriptions.</param>
        protected OperationInfo(MethodInfo underlyingMethod, Uri uri, string uriTemplate, Regex templateRegex, params ValueInfo[] values)
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
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }

                if (values.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("values");
                }
            }

            if (templateRegex == null)
            {
                throw new ArgumentNullException("templateRegex");
            }

            UnderlyingMethod = underlyingMethod;
            Uri = uri;
            UriTemplate = uriTemplate;
            TemplateRegex = templateRegex;
            var arguments = new List<ArgumentInfo>();
            var results = new List<ResultInfo>();
            Arguments = arguments;
            Results = results;
            var parameters = UnderlyingMethod.GetParameters();
            foreach (var value in values)
            {
                if ((!parameters.Contains(value.Parameter)) && (UnderlyingMethod.ReturnParameter != value.Parameter))
                {
                    throw new ArgumentOutOfRangeException("values");
                }

                value.Method = UnderlyingMethod;
                if (value is ResultInfo)
                {
                    results.Add((ResultInfo)value);
                }
                else
                {
                    arguments.Add((ArgumentInfo)value);
                }
            }
        }

        /// <summary>Gets the actual underlying method.</summary>
        public MethodInfo UnderlyingMethod { get; private set; }

        /// <summary>Gets the base uri of the method without arguments.</summary>
        public Uri Uri { get; private set; }

        /// <summary>Gets the uri template with all arguments included.</summary>
        public string UriTemplate { get; private set; }

        /// <summary>Gets the uri regular expression template with all arguments included.</summary>
        public Regex TemplateRegex { get; private set; }

        /// <summary>Gets the argument descriptions.</summary>
        public IEnumerable<ArgumentInfo> Arguments { get; private set; }

        /// <summary>Gets the result descriptions.</summary>
        public IEnumerable<ResultInfo> Results { get; private set; }
    }

    /// <summary>Describes an controller operation.</summary>
    /// <typeparam name="T">Type of the protocol specific command.</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Generic and non-generic interfaces. Suppression is OK.")]
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{ProtocolSpecificCommand} {UriTemplate}", Name = "{Uri}")]
    public class OperationInfo<T> : OperationInfo
    {
        /// <summary>Initializes a new instance of the <see cref="OperationInfo{T}" /> class.</summary>
        /// <param name="underlyingMethod">Actual underlying method.</param>
        /// <param name="uri">Base relative uri of the method without arguments.</param>
        /// <param name="uriTemplate">Relative uri template with all arguments included.</param>
        /// <param name="templateRegex">Regular expression template with all arguments included.</param>
        /// <param name="protocolSpecificCommand">Protocol specific command.</param>
        /// <param name="values">Values descriptions.</param>
        public OperationInfo(MethodInfo underlyingMethod, Uri uri, string uriTemplate, Regex templateRegex, T protocolSpecificCommand, params ValueInfo[] values)
            : base(underlyingMethod, uri, uriTemplate, templateRegex, values)
        {
            if (protocolSpecificCommand == null)
            {
                throw new ArgumentNullException("protocolSpecificCommand");
            }

            ProtocolSpecificCommand = protocolSpecificCommand;
        }

        /// <summary>Gets the protocol specific command.</summary>
        public T ProtocolSpecificCommand { get; private set; }
    }
}