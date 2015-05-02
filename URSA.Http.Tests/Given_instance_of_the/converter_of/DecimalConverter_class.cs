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
    public class DecimalConverter_class : ConverterTest<NumberConverter, decimal>
    {
        private const decimal Entity = 1.0M;
        private static readonly decimal[] Entities = { 1.0M, 2.0M };

        protected override decimal SingleEntity { get { return Entity; } }

        protected override decimal[] MultipleEntities { get { return Entities; } }
    }
}