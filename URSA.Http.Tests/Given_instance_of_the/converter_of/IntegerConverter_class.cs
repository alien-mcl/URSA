using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class IntegerConverter_class : ConverterTest<NumberConverter, int>
    {
        private const int Entity = 1;
        private static readonly int[] Entities = { 1, 2 };

        protected override int SingleEntity { get { return Entity; } }

        protected override int[] MultipleEntities { get { return Entities; } }
    }
}