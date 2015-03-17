using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Description
{
    /// <summary>Describes a parameter.</summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Parameter}", Name = "{Parameter}")]
    public abstract class ValueInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ValueInfo" /> class.</summary>
        /// <param name="parameter">Actual underlying parameter.</param>
        /// <param name="uriTemplate">Relative uri template of this value.</param>
        /// <param name="variableName">Variable name in the template for given value</param>
        public ValueInfo(ParameterInfo parameter, string uriTemplate, string variableName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (!String.IsNullOrEmpty(variableName))
            {
                if (uriTemplate == null)
                {
                    throw new ArgumentNullException("uriTemplate");
                }

                if (uriTemplate.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("uriTemplate");
                }
            }

            if (!String.IsNullOrEmpty(uriTemplate))
            {
                if (variableName == null)
                {
                    throw new ArgumentNullException("variableName");
                }

                if (variableName.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("variableName");
                }
            }

            Parameter = parameter;
            UriTemplate = uriTemplate;
            VariableName = variableName;
        }

        /// <summary>Gets the underlying parameter.</summary>
        public ParameterInfo Parameter { get; private set; }

        /// <summary>Gets the relative uri template of this value.</summary>
        public string UriTemplate { get; private set; }

        /// <summary>Gets the variable name in the template for given value.</summary>
        public string VariableName { get; private set; }

        /// <summary>Gets or sets the owning method.</summary>
        internal MethodInfo Method { get; set; }
    }
}