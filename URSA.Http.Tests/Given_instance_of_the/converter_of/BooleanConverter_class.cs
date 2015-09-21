using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class BooleanConverter_class : ConverterTest<BooleanConverter, bool>
    {
        private const bool Entity = false;
        private static readonly bool[] Entities = { false, true };

        protected override bool SingleEntity { get { return Entity; } }

        protected override bool[] MultipleEntities { get { return Entities; } }
    }
}