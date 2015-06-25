using System;
using System.Collections.Generic;
using Castle.MicroKernel;

namespace URSA.CastleWindsor.ComponentModel
{
    internal class HandlerEqualityComparer : IEqualityComparer<IHandler>
    {
        internal static readonly HandlerEqualityComparer Instance = new HandlerEqualityComparer();

        private HandlerEqualityComparer()
        {
        }

        /// <inheritdoc />
        public bool Equals(IHandler x, IHandler y)
        {
            return (Object.Equals(x, null) && (Object.Equals(y, null))) ||
                ((!Object.Equals(x, null)) && (!Object.Equals(y, null)) && (x.ComponentModel.Implementation == y.ComponentModel.Implementation));
        }

        /// <inheritdoc />
        public int GetHashCode(IHandler obj)
        {
            return obj.ComponentModel.Implementation.GetHashCode();
        }
    }
}