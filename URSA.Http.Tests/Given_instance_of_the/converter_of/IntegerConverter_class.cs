using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class IntegerConverter_class : ConverterTest<NumberConverter, int>
    {
        private const int Entity = 1;
        private static readonly int[] Entities = { 1, 2 };

        protected override int SingleEntity { get { return Entity; } }

        protected override int[] MultipleEntities { get { return Entities; } }
    }
}