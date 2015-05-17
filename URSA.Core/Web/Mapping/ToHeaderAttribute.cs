using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Mapping
{
    /// <summary>Marks the parameter to be bound to the response header.</summary>
    [ExcludeFromCodeCoverage]
    public class ToHeaderAttribute : ResultTargetAttribute
    {
        /// <summary>Initializes a new instance of the <see cref="ToHeaderAttribute" /> class.</summary>
        /// <param name="name">Name of the header to be used.</param>
        public ToHeaderAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (name.Length == 0)
            {
                throw new ArgumentOutOfRangeException("name");
            }

            Name = name;
        }

        /// <summary>Gets the name of the header to be used.</summary>
        public string Name { get; private set; }
    }
}