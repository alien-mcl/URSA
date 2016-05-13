using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http;

namespace URSA.Web.Description
{
    /// <summary>Describes an entry point;</summary>
    [ExcludeFromCodeCoverage]
    [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
    public class EntryPointInfo : SecurableResourceInfo
    {
        /// <summary>Initializes a new instance of the <see cref="EntryPointInfo"/> class.</summary>
        /// <param name="url">Entry point URL.</param>
        public EntryPointInfo(Url url) : base(url)
        {
        }

        /// <inheritdoc />
        public override SecurableResourceInfo Owner { get { return null; } }

        /// <summary>Performs an implicit conversion from <see cref="EntryPointInfo"/> to <see cref="Url"/>.</summary>
        /// <param name="entryPoint">The entry point.</param>
        /// <returns>URL of the entry point.</returns>
        public static implicit operator Url(EntryPointInfo entryPoint)
        {
            return (entryPoint != null ? entryPoint.Url : null);
        }
    }
}