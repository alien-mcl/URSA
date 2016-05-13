using System;
using System.Diagnostics;
using System.Reflection;

namespace URSA.Web.Description
{
    /// <summary>Describes a parameter.</summary>
    [DebuggerDisplay("{Parameter}", Name = "{Parameter}")]
    public abstract class ValueInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ValueInfo" /> class.</summary>
        /// <param name="parameter">Actual underlying parameter.</param>
        /// <param name="urlTemplate">Relative URL template of this value.</param>
        /// <param name="variableName">Variable name in the template for given value</param>
        protected ValueInfo(ParameterInfo parameter, string urlTemplate, string variableName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (!String.IsNullOrEmpty(variableName))
            {
                if (urlTemplate == null)
                {
                    throw new ArgumentNullException("urlTemplate");
                }

                if (urlTemplate.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("urlTemplate");
                }
            }

            if (!String.IsNullOrEmpty(urlTemplate))
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
            UrlTemplate = urlTemplate;
            VariableName = variableName;
        }

        /// <summary>Gets the underlying parameter.</summary>
        public ParameterInfo Parameter { get; private set; }

        /// <summary>Gets the relative URL template of this value.</summary>
        public string UrlTemplate { get; private set; }

        /// <summary>Gets the variable name in the template for given value.</summary>
        public string VariableName { get; private set; }

        /// <summary>Gets or sets the owning method.</summary>
        internal MethodInfo Method { get; set; }
    }
}