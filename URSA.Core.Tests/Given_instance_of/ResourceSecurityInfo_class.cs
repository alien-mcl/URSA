using System;
using FluentAssertions;
using NUnit.Framework;
using URSA.Security;

namespace Given_instance_of
{
    [TestFixture]
    public class ResourceSecurityInfo_class
    {
        private const string ExpectedClaim = "claim";
        private const string ExpectedValue = "value";

        private ResourceSecurityInfo _resourceSecurityInfo;

        [Test]
        public void it_should_throw_when_no_claim_is_passed_when_allowing()
        {
            _resourceSecurityInfo.Invoking(instance => instance.Allow(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("claimType");
        }

        [Test]
        public void it_should_throw_when_no_claim_is_passed_when_disallowing()
        {
            _resourceSecurityInfo.Invoking(instance => instance.Disallow(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("claimType");
        }

        [Test]
        public void it_should_throw_when_no_claim_is_passed_when_denying()
        {
            _resourceSecurityInfo.Invoking(instance => instance.Deny(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("claimType");
        }

        [Test]
        public void it_should_throw_when_no_claim_is_passed_when_undenying()
        {
            _resourceSecurityInfo.Invoking(instance => instance.Undeny(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("claimType");
        }

        [Test]
        public void it_should_throw_when_no_other_settings_are_passed_when_overriding()
        {
            _resourceSecurityInfo.Invoking(instance => instance.OverrideWith(null)).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("specificSecurityInfo");
        }

        [Test]
        public void it_should_allow_a_claim()
        {
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [Test]
        public void it_should_disallow_a_claim()
        {
            _resourceSecurityInfo
                .Allow(ExpectedClaim, ExpectedValue)
                .Disallow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().BeNull();
        }

        [Test]
        public void it_should_not_deny_a_claim_simultanously()
        {
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Denied[ExpectedClaim].Should().BeNull();
        }

        [Test]
        public void it_should_deny_a_claim()
        {
            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Denied[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [Test]
        public void it_should_undeny_a_claim()
        {
            _resourceSecurityInfo
                .Deny(ExpectedClaim, ExpectedValue)
                .Undeny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Denied[ExpectedClaim].Should().BeNull();
        }

        [Test]
        public void it_should_not_allow_a_claim_simultanously()
        {
            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().BeNull();
        }

        [Test]
        public void it_should_deny_an_allowed_claim()
        {
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().BeNull();
            _resourceSecurityInfo.Denied[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [Test]
        public void it_should_allow_a_denied_claim()
        {
            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().Contain(ExpectedValue);
            _resourceSecurityInfo.Denied[ExpectedClaim].Should().BeNull();
        }

        [Test]
        public void it_should_merge_specifications()
        {
            var other = new ResourceSecurityInfo().Deny(ExpectedClaim, ExpectedValue);
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo = _resourceSecurityInfo.OverrideWith(other);

            _resourceSecurityInfo.Allowed.Should().BeEmpty();
            _resourceSecurityInfo.Denied[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [SetUp]
        public void Setup()
        {
            _resourceSecurityInfo = new ResourceSecurityInfo();
        }

        [TearDown]
        public void Teardown()
        {
            _resourceSecurityInfo = null;
        }
    }
}