using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Description
{
    /// <summary>Describes a parameter.</summary>
    [ExcludeFromCodeCoverage]
    public class ResultInfo : ValueInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ResultInfo" /> class.</summary>
        /// <param name="parameter">Actual underlying parameter.</param>
        /// <param name="target">Result target.</param>
        /// <param name="uriTemplate">Relative uri template of this result.</param>
        /// <param name="variableName">Variable name in the template for given result</param>
        public ResultInfo(ParameterInfo parameter, ResultTargetAttribute target, string uriTemplate, string variableName) : base(parameter, uriTemplate, variableName)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Target = target;
        }

        /// <summary>Gets the result target.</summary>
        public ResultTargetAttribute Target { get; private set; }
    }
}