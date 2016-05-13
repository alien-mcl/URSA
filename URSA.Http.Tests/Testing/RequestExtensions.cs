using System;
using System.IO;
using System.Linq;
using URSA.Security;

namespace URSA.Web.Http.Testing
{
    internal static class RequestExtensions
    {
        internal const string DefaultHost = "temp.uri";

        internal static RequestInfo CreateRequest(this object unitTest, params Header[] headers)
        {
            return new RequestInfo(Verb.GET, (HttpUrl)UrlParser.Parse("http://" + DefaultHost + "/"), new MemoryStream(), new BasicClaimBasedIdentity(), headers);
        }

        internal static ResponseInfo CreateResponse(this object unitTest, params Header[] headers)
        {
            return new StringResponseInfo(String.Empty, unitTest.CreateRequest(headers));
        }

        internal static ResponseInfo CreateResponseWithOrigin(this object unitTest, string origin = "localhost", params Header[] headers)
        {
            return new StringResponseInfo(String.Empty, unitTest.CreateRequest((headers ?? new Header[0]).Concat(new[] { new Header("Origin", origin) }).ToArray()));
        }
    }
}