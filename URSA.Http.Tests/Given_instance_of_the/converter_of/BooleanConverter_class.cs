using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class BooleanConverter_class : ConverterTest<BooleanConverter, bool>
    {
        private const bool Entity = false;
        private static readonly bool[] Entities = { false, true };

        protected override bool SingleEntity { get { return Entity; } }

        protected override bool[] MultipleEntities { get { return Entities; } }
    }
}