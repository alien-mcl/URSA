using System;
using System.Reflection;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a <see cref="DateTime" />.</summary>
    public class DateTimeConverter : SpecializedLiteralConverter<DateTime>
    {
        /// <inheritdoc />
        protected override int MaxBodyLength { get { return 28; } }

        /// <inheritdoc />
        protected override bool CanConvert(Type expectedType)
        {
            return expectedType.GetItemType() == typeof(DateTime);
        }
    }
}