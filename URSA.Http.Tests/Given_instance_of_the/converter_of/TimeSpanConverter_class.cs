using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Testing;

namespace Given_instance_of_the.converter_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class TimeSpanConverter_class : ConverterTest<TimeSpanConverter, TimeSpan>
    {
        private static readonly TimeSpan Entity = DateTime.Now.TimeOfDay;
        private static readonly TimeSpan[] Entities = { DateTime.Now.TimeOfDay, TimeSpan.FromHours(1) };

        protected override TimeSpan SingleEntity { get { return Entity; } }

        protected override TimeSpan[] MultipleEntities { get { return Entities; } }
    }
}