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
            var itemType = expectedType.GetTypeInfo().GetItemType();
            return (itemType == typeof(byte)) || (itemType == typeof(sbyte)) ||
                (itemType == typeof(ushort)) || (itemType == typeof(short)) ||
                (itemType == typeof(uint)) || (itemType == typeof(int)) ||
                (itemType == typeof(ulong)) || (itemType == typeof(long)) ||
                (itemType == typeof(float)) || (itemType == typeof(double)) ||
                (itemType == typeof(decimal));
        }
    }
}