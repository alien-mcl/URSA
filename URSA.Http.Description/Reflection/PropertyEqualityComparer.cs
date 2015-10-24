using System.Collections.Generic;
using System.Reflection;

namespace URSA.Web.Http.Description.Reflection
{
    internal class PropertyEqualityComparer : IEqualityComparer<PropertyInfo>
    {
        internal static readonly PropertyEqualityComparer Default = new PropertyEqualityComparer();

        private PropertyEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return (x.Name == y.Name) && (x.PropertyType == y.PropertyType);
        }

        /// <inheritdoc />
        public int GetHashCode(PropertyInfo obj)
        {
            return obj.Name.GetHashCode() ^ obj.PropertyType.GetHashCode();
        }
    }
}