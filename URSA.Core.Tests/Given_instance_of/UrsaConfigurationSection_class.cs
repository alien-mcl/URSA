using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using URSA.Configuration;
using URSA.Web.Converters;

namespace Given_instance_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class UrsaConfigurationSection_class
    {
        [Test]
        public void it_should_throw_when_no_provider_type_provided()
        {
            ArgumentNullException exception = null;
            try
            {
                UrsaConfigurationSection.GetProvider<IConverterProvider>(null);
            }
            catch (ArgumentNullException error)
            {
                exception = error;
            }

            exception.Should().NotBeNull();
        }

        [Test]
        public void it_should_throw_when_provided_type_is_not_of_required_provider_base_type()
        {
            ArgumentOutOfRangeException exception = null;
            try
            {
                UrsaConfigurationSection.GetProvider<IConverterProvider>(typeof(object));
            }
            catch (ArgumentOutOfRangeException error)
            {
                exception = error;
            }

            exception.Should().NotBeNull();
        }

        [Test]
        public void it_should_throw_when_invalid_constructor_arguments_are_passed()
        {
            InvalidOperationException exception = null;
            try
            {
                UrsaConfigurationSection.GetProvider<IConverterProvider>(typeof(DefaultConverterProvider), typeof(object));
            }
            catch (InvalidOperationException error)
            {
                exception = error;
            }

            exception.Should().NotBeNull();
        }

        [Test]
        public void it_should_create_a_provider_of_given_type()
        {
            var providerType = typeof(DefaultConverterProvider);

            var provider = UrsaConfigurationSection.GetProvider<IConverterProvider>(providerType);

            provider.Should().NotBeNull();
            provider.DeclaringType.Should().Be(providerType);
        }
    }
}