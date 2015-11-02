using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Mapping
{
    /// <summary>Marks the parameter to be bound to the response header.</summary>
    [ExcludeFromCodeCoverage]
    public class ToHeaderAttribute : ResultTargetAttribute
    {
        private const string DefaultFormat = "{0}";

        /// <summary>Initializes a new instance of the <see cref="ToHeaderAttribute" /> class.</summary>
        /// <param name="name">Name of the header to be used.</param>
        /// <param name="format">Optional format string of the header value. Value of "{0}" is used if none provided.</param>
        public ToHeaderAttribute(string name, string format = DefaultFormat)
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
            Format = format ?? DefaultFormat;
        }

        /// <summary>Gets the name of the header to be used.</summary>
        public string Name { get; private set; }

        /// <summary>Gets the format string of the header value.</summary>
        public string Format { get; private set; }
    }
}