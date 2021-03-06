﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Http.Mapping
{
    /// <summary>Instructs the pipeline to map the method to a PUT HTTP verb.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    [AttributeUsage(AttributeTargets.Method)]
    public class OnPutAttribute : OnVerbAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="OnPutAttribute" /> class.</summary>
        public OnPutAttribute() : base(Verb.PUT)
        {
        }
    }
}