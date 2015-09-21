using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Description
{
    /// <summary>Describes a parameter.</summary>
    [ExcludeFromCodeCoverage]
    public class ArgumentInfo : ValueInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ArgumentInfo" /> class.</summary>
        /// <param name="parameter">Actual underlying parameter.</param>
        /// <param name="source">Parameter source.</param>
        /// <param name="uriTemplate">Relative uri template of this parameter.</param>
        /// <param name="variableName">Variable name in the template for given argument</param>
        public ArgumentInfo(ParameterInfo parameter, ParameterSourceAttribute source, string uriTemplate, string variableName) : base(parameter, uriTemplate, variableName)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Source = source;
        }

        /// <summary>Gets the parameter source.</summary>
        public ParameterSourceAttribute Source { get; private set; }
    }
}