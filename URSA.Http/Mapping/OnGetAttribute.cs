using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Instructs the pipeline to map the method to a GET HTTP verb.</summary>
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method)]
    public class OnGetAttribute : OnVerbAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="OnGetAttribute" /> class.</summary>
        public OnGetAttribute() : base(Verb.GET)
        {
        }
    }
}