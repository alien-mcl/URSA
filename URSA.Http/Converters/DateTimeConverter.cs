using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace URSA.Web.Http.Converters
{
    /// <summary>Converts HTTP body into a <see cref="DateTime" />.</summary>
    public class DateTimeConverter : SpecializedLiteralConverter<DateTime>
    {
        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "No testable logic.")]
        protected override int MaxBodyLength { get { return 28; } }

        /// <inheritdoc />
        protected override bool CanConvert(Type expectedType)
        {
            return expectedType.GetTypeInfo().GetItemType() == typeof(DateTime);
        }
    }
}