using System;
using System.Reflection;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a <see cref="TimeSpan" />.</summary>
    public class TimeSpanConverter : SpecializedLiteralConverter<TimeSpan>
    {
        /// <inheritdoc />
        protected override int MaxBodyLength { get { return 16; } }

        /// <inheritdoc />
        protected override bool CanConvert(Type expectedType)
        {
            return expectedType.GetTypeInfo().GetItemType() == typeof(TimeSpan);
        }
    }
}