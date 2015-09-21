using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class StringConverter_class : ConverterTest<StringConverter, string>
    {
        private const string Entity = "test string";
        private static readonly string[] Entities = { "test string 1", "test string 2" };

        protected override string SingleEntity { get { return Entity; } }

        protected override string[] MultipleEntities { get { return Entities; } }
    }
}