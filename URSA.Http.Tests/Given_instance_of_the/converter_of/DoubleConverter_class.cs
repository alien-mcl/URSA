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
    public class DoubleConverter_class : ConverterTest<NumberConverter, double>
    {
        private const double Entity = 1.0;
        private static readonly double[] Entities = { 1.0, 2.0 };

        protected override double SingleEntity { get { return Entity; } }

        protected override double[] MultipleEntities { get { return Entities; } }
    }
}