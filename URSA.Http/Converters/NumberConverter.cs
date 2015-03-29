using System;
using System.Reflection;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a number.</summary>
    public class NumberConverter : SpecializedLiteralConverter<decimal>
    {
        /// <inheritdoc />
        protected override int MaxBodyLength { get { return 32; } }

        /// <inheritdoc />
        protected override bool CanConvert(Type expectedType)
        {
            return (expectedType.GetItemType() == typeof(byte)) || (expectedType.GetItemType() == typeof(sbyte)) ||
                (expectedType.GetItemType() == typeof(ushort)) || (expectedType.GetItemType() == typeof(short)) ||
                (expectedType.GetItemType() == typeof(uint)) || (expectedType.GetItemType() == typeof(int)) ||
                (expectedType.GetItemType() == typeof(ulong)) || (expectedType.GetItemType() == typeof(long)) ||
                (expectedType.GetItemType() == typeof(float)) || (expectedType.GetItemType() == typeof(double)) ||
                (expectedType.GetItemType() == typeof(decimal));
        }
    }
}