using System;
using System.Diagnostics.CodeAnalysis;

namespace URSA.Web.Description
{
    /// <summary>Describes an entry point;</summary>
    [ExcludeFromCodeCoverage]
    public class EntryPointInfo : SecurableResourceInfo
    {
        /// <summary>Initializes a new instance of the <see cref="EntryPointInfo"/> class.</summary>
        /// <param name="uri">Entry point uri.</param>
        public EntryPointInfo(Uri uri) : base(uri)
        {
        }

        /// <inheritdoc />
        public override SecurableResourceInfo Owner { get { return null; } }

        /// <summary>Performs an implicit conversion from <see cref="EntryPointInfo"/> to <see cref="Uri"/>.</summary>
        /// <param name="entryPoint">The entry point.</param>
        /// <returns>Uri of the entry point.</returns>
        public static implicit operator Uri(EntryPointInfo entryPoint)
        {
            return (entryPoint != null ? entryPoint.Uri : null);
        }
    }
}