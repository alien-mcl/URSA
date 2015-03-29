using System;
using System.Collections.Generic;

namespace URSA.Web.Http.Description.Hydra
{
    /// <summary>Compares two <see cref="IOperation" />s</summary>
    public class OperationComparer : IEqualityComparer<IOperation>
    {
        /// <summary>Exposes a default instance of the <see cref="OperationComparer" />.</summary>
        public static readonly OperationComparer Default = new OperationComparer();

        private OperationComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(IOperation x, IOperation y)
        {
            return ((Equals(x, null)) && (Equals(y, null))) || (((!Equals(x, null)) && (!Equals(y, null))) && (x.Id == y.Id));
        }

        /// <inheritdoc />
        public int GetHashCode(IOperation obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return obj.Id.GetHashCode();
        }
    }
}