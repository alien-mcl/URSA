using System.Collections.Generic;
using URSA.Web.Http;

namespace URSA.Testing
{
    public class RelativeUrlParser : UrlParser
    {
        public override IEnumerable<string> SupportedSchemes { get { return new string[0]; } }

        public override bool AllowsRelativeAddresses { get { return true; } }

        public override Url Parse(string url, int schemeSpecificPartIndex)
        {
            return new RelativeUrl(url);
        }
    }
}