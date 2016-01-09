using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Security;

namespace Given_instance_of
{
    [TestClass]
    public class ResourceSecurityInfo_class
    {
        private const string ExpectedClaim = "claim";
        private const string ExpectedValue = "value";

        private ResourceSecurityInfo _resourceSecurityInfo;

        [TestMethod]
        public void it_should_allow_a_claim()
        {
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [TestMethod]
        public void it_should_disallow_a_claim()
        {
            _resourceSecurityInfo
                .Allow(ExpectedClaim, ExpectedValue)
                .Disallow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().BeNull();
        }

        [TestMethod]
        public void it_should_not_deny_a_claim_simultanously()
        {
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Denied[ExpectedClaim].Should().BeNull();
        }

        [TestMethod]
        public void it_should_deny_a_claim()
        {
            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Denied[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [TestMethod]
        public void it_should_undeny_a_claim()
        {
            _resourceSecurityInfo
                .Deny(ExpectedClaim, ExpectedValue)
                .Undeny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Denied[ExpectedClaim].Should().BeNull();
        }

        [TestMethod]
        public void it_should_not_allow_a_claim_simultanously()
        {
            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().BeNull();
        }

        [TestMethod]
        public void it_should_deny_an_allowed_claim()
        {
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().BeNull();
            _resourceSecurityInfo.Denied[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [TestMethod]
        public void it_should_allow_a_denied_claim()
        {
            _resourceSecurityInfo.Deny(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo.Allowed[ExpectedClaim].Should().Contain(ExpectedValue);
            _resourceSecurityInfo.Denied[ExpectedClaim].Should().BeNull();
        }

        [TestMethod]
        public void it_should_merge_specifications()
        {
            var other = new ResourceSecurityInfo().Deny(ExpectedClaim, ExpectedValue);
            _resourceSecurityInfo.Allow(ExpectedClaim, ExpectedValue);

            _resourceSecurityInfo = _resourceSecurityInfo.OverrideWith(other);

            _resourceSecurityInfo.Allowed.Should().BeEmpty();
            _resourceSecurityInfo.Denied[ExpectedClaim].Should().Contain(ExpectedValue);
        }

        [TestInitialize]
        public void Setup()
        {
            _resourceSecurityInfo = new ResourceSecurityInfo();
        }

        [TestCleanup]
        public void Teardown()
        {
            _resourceSecurityInfo = null;
        }
    }
}