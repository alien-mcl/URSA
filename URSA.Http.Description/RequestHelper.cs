using System;
using System.Collections.Generic;
using System.Linq;

namespace URSA.Web.Http.Description
{
    internal static class RequestHelper
    {
        internal static IEnumerable<Uri> GetRequestedMediaTypeProfiles(this IController controller)
        {
            return controller.Response.Request.GetRequestedMediaTypeProfiles();
        }

        internal static IEnumerable<Uri> GetRequestedMediaTypeProfiles(this IRequestInfo request)
        {
            RequestInfo requestInfo = request as RequestInfo;
            if (requestInfo == null)
            {
                return new Uri[0];
            }

            //// TODO: Introduce strongly typed header/parameter parsing routines.
            IEnumerable<Uri> result = null;
            var accept = requestInfo.Headers[Header.Accept];
            if (accept != null)
            {
                result = from value in accept.Values from parameter in value.Parameters where parameter.Name == "profile" select (Uri)parameter.Value;
            }

            var link = requestInfo.Headers[Header.Link];
            if (link == null)
            {
                return result;
            }

            var linkProfile = from value in link.Values from parameter in value.Parameters where parameter.Name == "rel" select new Uri(value.Value);
            return (result == null ? linkProfile : result.Union(linkProfile));
        }
    }
}