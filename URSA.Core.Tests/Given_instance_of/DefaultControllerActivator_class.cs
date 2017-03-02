using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using URSA.ComponentModel;
using URSA.Web;

namespace Given_instance_of
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    public class DefaultControllerActivator_class
    {
        private Mock<IComponentProvider> _container;
        private DefaultControllerActivator _activator;

        [Test]
        public void it_should_create_basic_controller_instance()
        {
            var controller = new BasicController();
            _container.Setup(instance => instance.CanResolve(typeof(BasicController), null)).Returns(true);
            _container.Setup(instance => instance.Resolve(typeof(BasicController), null)).Returns(controller);

            var result = _activator.CreateInstance(typeof(BasicController));

            result.Should().Be(controller);
            _container.Verify(instance => instance.CanResolve(typeof(BasicController), null));
            _container.Verify(instance => instance.Resolve(typeof(BasicController), null));
        }

        [Test]
        public void it_should_create_generic_type_controller_instance()
        {
            var controller = new DescriptionController<BasicController>();
            _container.Setup(instance => instance.CanResolve(typeof(DescriptionController<BasicController>), null)).Returns(false);
            _container.Setup(instance => instance.CanResolve(typeof(DescriptionController<>), null)).Returns(true);
            _container.Setup(instance => instance.Resolve(typeof(DescriptionController<>), null)).Returns(controller);

            var result = _activator.CreateInstance(typeof(DescriptionController<BasicController>));

            result.Should().Be(controller);
            _container.Verify(instance => instance.CanResolve(typeof(DescriptionController<BasicController>), null));
            _container.Verify(instance => instance.CanResolve(typeof(DescriptionController<>), null));
            _container.Verify(instance => instance.Resolve(typeof(DescriptionController<>), null));
        }

        [Test]
        public void it_should_create_complex_controller_instance()
        {
            var controller = new BasicController();
            _container.Setup(instance => instance.CanResolve(typeof(BasicController), null)).Returns(false);
            _container.Setup(instance => instance.Resolve(typeof(IController), null)).Returns(controller);

            var result = _activator.CreateInstance(typeof(BasicController));

            result.Should().Be(controller);
            _container.Verify(instance => instance.CanResolve(typeof(BasicController), null));
            _container.Verify(instance => instance.Resolve(typeof(IController), null));
        }

        [SetUp]
        public void Setup()
        {
            _container = new Mock<IComponentProvider>(MockBehavior.Strict);
            _activator = new DefaultControllerActivator(_container.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _container = null;
            _activator = null;
        }

        private class BasicController : IController
        {
            public IResponseInfo Response { get; set; }
        }

        private class DescriptionController<T> : IController where T : IController
        {
            public IResponseInfo Response { get; set; }
        }
    }
}