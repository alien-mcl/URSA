using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using URSA.Web.Mapping;

namespace URSA.Web.Description
{
    /// <summary>Describes a parameter.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    public class ResultInfo : ValueInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ResultInfo" /> class.</summary>
        /// <param name="parameter">Actual underlying parameter.</param>
        /// <param name="target">Result target.</param>
        /// <param name="urlTemplate">Relative URL template of this result.</param>
        /// <param name="variableName">Variable name in the template for given result</param>
        public ResultInfo(ParameterInfo parameter, ResultTargetAttribute target, string urlTemplate, string variableName) : base(parameter, urlTemplate, variableName)
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