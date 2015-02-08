using System;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a <see cref="Guid" />.</summary>
    public class GuidConverter : SpecializedLiteralConverter<Guid>
    {
        /// <inheritdoc />
        protected override int MaxBodyLength { get { return 38; } }

        /// <inheritdoc />
        protected override bool CanConvert(Type expectedType)
        {
            return expectedType.GetItemType() == typeof(Guid);
        }
    }
}