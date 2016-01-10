using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

namespace URSA.Web.Description
{
    /// <summary>Describes an controller operation.</summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{UriTemplate}", Name = "{Uri}")]
    public abstract class OperationInfo : SecurableResourceInfo
    {
        /// <summary>Initializes a new instance of the <see cref="OperationInfo" /> class.</summary>
        /// <param name="underlyingMethod">Actual underlying method.</param>
        /// <param name="uri">Base relative uri of the method without arguments.</param>
        /// <param name="uriTemplate">Relative uri template with all arguments included.</param>
        /// <param name="templateRegex">Regular expression template with all arguments included.</param>
        /// <param name="values">Values descriptions.</param>
        protected OperationInfo(MethodInfo underlyingMethod, Uri uri, string uriTemplate, Regex templateRegex, params ValueInfo[] values) : base(uri)
        {
            if (underlyingMethod == null)
            {
                throw new ArgumentNullException("underlyingMethod");
            }

            if (!typeof(IController).IsAssignableFrom(underlyingMethod.DeclaringType))
            {
                throw new ArgumentOutOfRangeException("underlyingMethod");
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
            UriTemplate = uriTemplate;
            TemplateRegex = templateRegex;
            var arguments = new List<ArgumentInfo>();
            var results = new List<ResultInfo>();
            Arguments = arguments;
            Results = results;
            foreach (var value in values)
            {
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

        /// <summary>Gets the uri template with all arguments included.</summary>
        public string UriTemplate { get; private set; }

        /// <summary>Gets the uri regular expression template with all arguments included.</summary>
        public Regex TemplateRegex { get; private set; }

        /// <summary>Gets the argument descriptions.</summary>
        public IEnumerable<ArgumentInfo> Arguments { get; private set; }

        /// <summary>Gets the result descriptions.</summary>
        public IEnumerable<ResultInfo> Results { get; private set; }

        /// <inheritdoc />
        public override SecurableResourceInfo Owner { get { return Controller; } }

        /// <summary>Gets or sets the controller descriptor.</summary>
        public ControllerInfo Controller { get; set; }
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

        /// <summary>Implements the operator equality.</summary>
        /// <param name="operandA">Left operand.</param>
        /// <param name="operandB">Right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(OperationInfo<T> operandA, OperationInfo<T> operandB)
        {
            return ((Equals(operandA, null)) && (Equals(operandB, null))) || ((!Equals(operandA, null)) && (!Equals(operandB, null)) &&
                (operandA.UnderlyingMethod.Equals(operandB.UnderlyingMethod)) && (operandA.ProtocolSpecificCommand.Equals(operandB.ProtocolSpecificCommand)) &&
                (operandA.Uri.ToString().Equals(operandB.Uri.ToString())));
        }

        /// <summary>Implements the operator inequality operator.</summary>
        /// <param name="operandA">Left operand.</param>
        /// <param name="operandB">Right operand.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(OperationInfo<T> operandA, OperationInfo<T> operandB)
        {
            return ((Equals(operandA, null)) && (!Equals(operandB, null))) ||
                ((!Equals(operandA, null)) && (Equals(operandB, null))) || 
                ((!Equals(operandA, null)) && (!Equals(operandB, null)) &&
                ((!operandA.UnderlyingMethod.Equals(operandB.UnderlyingMethod)) || (!operandA.ProtocolSpecificCommand.Equals(operandB.ProtocolSpecificCommand)) ||
                (!operandA.Uri.ToString().Equals(operandB.Uri.ToString()))));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return UnderlyingMethod.GetHashCode() ^ Uri.ToString().GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if ((Equals(obj, null)) || (obj.GetType() != typeof(OperationInfo<T>)))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var operation = (OperationInfo<T>)obj;
            return (UnderlyingMethod.Equals(operation.UnderlyingMethod)) && (ProtocolSpecificCommand.Equals(operation.ProtocolSpecificCommand)) &&
                (Uri.ToString().Equals(operation.Uri.ToString()));
        }
    }
}