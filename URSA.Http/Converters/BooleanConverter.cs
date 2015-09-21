using System;
using System.Reflection;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a <see cref="bool" />.</summary>
    public class BooleanConverter : SpecializedLiteralConverter<bool>
    {
        /// <inheritdoc />
        protected override int MaxBodyLength { get { return 6; } }

        /// <inheritdoc />
        protected override bool CanConvert(Type expectedType)
        {
            return expectedType.GetItemType() == typeof(bool);
        }
    }
}