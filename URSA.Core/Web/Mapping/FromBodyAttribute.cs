using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace URSA.Web.Mapping
{
    /// <summary>Marks the parameter to be bound to the body of the request.</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    public sealed class FromBodyAttribute : ParameterSourceAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="FromBodyAttribute" /> class.</summary>
        public FromBodyAttribute()
        {
            Name = String.Empty;
        }

        /// <summary>Initializes a new instance of the <see cref="FromBodyAttribute" /> class.</summary>
        /// <param name="name">Name of the parameter in the multipart body form data.</param>
        public FromBodyAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            Name = name;
        }

        /// <summary>Gets the name of the parameter in the multipart body form data.</summary>
        public string Name { get; private set; }

        /// <summary>Creates a body parameter source mapping for given parameter.</summary>
        /// <param name="parameter">Parameter for which to create mapping.</param>
        /// <returns>Instance of the <see cref="FromBodyAttribute" />.</returns>
        public static FromBodyAttribute For(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            return new FromBodyAttribute(parameter.Name);
        }
    }
}
