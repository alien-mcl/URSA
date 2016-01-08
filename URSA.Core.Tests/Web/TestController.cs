using System.Security.Claims;
using URSA.Security;
using URSA.Web;

namespace URSA.Tests.Web
{
    [AllowClaim(ClaimTypes.Name, "test")]
    [DenyClaim(ClaimTypes.Name, "other")]
    public class TestController : IController
    {
        public IResponseInfo Response { get; set; }

        [AllowClaim(ClaimTypes.Name, "other")]
        public void Operation()
        {
        }
    }
}