using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DateTimeConverter_class : ConverterTest<DateTimeConverter, DateTime>
    {
        private static readonly DateTime Entity = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        private static readonly DateTime[] Entities = { Entity, Entity.AddMinutes(1) };

        protected override DateTime SingleEntity { get { return Entity; } }

        protected override DateTime[] MultipleEntities { get { return Entities; } }
    }
}