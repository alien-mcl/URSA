using System.Collections.Generic;

namespace System
{
    /// <summary>Compares <see cref="Uri" />s literally.</summary>
    public class LiteralUriComparer : IEqualityComparer<Uri>
    {
        /// <summary>Defines an singleton instance of the <see cref="LiteralUriComparer"/>.</summary>
        public static readonly LiteralUriComparer Instance = new LiteralUriComparer();

        private IEqualityComparer<string> _comparer = StringComparer.OrdinalIgnoreCase;

        private LiteralUriComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(Uri x, Uri y)
        {
            return ((Object.Equals(x, null)) && (Object.Equals(y, null))) ||
                ((!Object.Equals(x, null)) && (!Object.Equals(y, null)) && (_comparer.Equals(x.ToString(), y.ToString())));
        }

        /// <inheritdoc />
        public int GetHashCode(Uri obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return obj.ToString().GetHashCode();
        }
    }
}