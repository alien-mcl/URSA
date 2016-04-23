using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Instructs the pipeline to map the method to a OPTIONS HTTP verb.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    [AttributeUsage(AttributeTargets.Method)]
    public class OnOptionsAttribute : OnVerbAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="OnOptionsAttribute" /> class.</summary>
        public OnOptionsAttribute() : base(Verb.OPTIONS)
        {
        }
    }
}