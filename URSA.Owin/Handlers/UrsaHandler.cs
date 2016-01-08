using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Owin;
using URSA.Configuration;
using URSA.Owin.Security;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;

namespace URSA.Owin.Handlers
{
    /// <summary>Provides a connection between URSA framework and Owin pipeline.</summary>
    public class UrsaHandler : OwinMiddleware
    {
        private readonly IRequestHandler<RequestInfo, ResponseInfo> _requestHandler;

        /// <summary>Initializes a new instance of the <see cref="UrsaHandler"/> class.</summary>
        /// <param name="next">Next middleware in the pipeline.</param>
        /// <param name="requestHandler">The request handler.</param>
        public UrsaHandler(OwinMiddleware next, IRequestHandler<RequestInfo, ResponseInfo> requestHandler) : base(next)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException("requestHandler");
            }

            _requestHandler = requestHandler;
        }

        /// <inheritdoc />
        public override async Task Invoke(IOwinContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if ((context.Request == null) || (context.Response == null))
            {
                throw new ArgumentOutOfRangeException("context");
            }

            if (LazyHttpServerConfiguration.HostingUri == null)
            {
                LazyHttpServerConfiguration.HostingUri = new Uri(context.Request.Uri.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
            }

            await ProcessRequest(context);
        }

        private async Task ProcessRequest(IOwinContext context)
        {
            if (context.Request.Uri.Segments.Any())
            {
                var segment = context.Request.Uri.Segments.Last();
                switch (segment)
                {
                    case EntityConverter.DocumentationStylesheet:
                        await HandleEmbeddedResource(context, "URSA.Web.Http.Description.DocumentationStylesheet.xslt", "text/xsl");
                        return;
                    case EntityConverter.PropertyIcon:
                        await HandleEmbeddedResource(context, "URSA.Web.Http.Description.Property.png", "image/png");
                        return;
                    case EntityConverter.MethodIcon:
                        await HandleEmbeddedResource(context, "URSA.Web.Http.Description.Method.png", "image/png");
                        return;
                }
            }

            await HandleRequest(context);
        }

        private static bool IsResponseNoMatchingRouteFoundException(ResponseInfo response)
        {
            return ((response.Status == HttpStatusCode.NotFound) && (response is ExceptionResponseInfo) &&
                (((ExceptionResponseInfo)response).Value is NoMatchingRouteFoundException));
        }

        private async Task HandleRequest(IOwinContext context)
        {
            var headers = new HeaderCollection();
            context.Request.Headers.ForEach(header => headers[header.Key] = new Header(header.Key, header.Value));
            if (!String.IsNullOrEmpty(context.Request.Headers[Header.Origin]))
            {
                context.Response.Headers[Header.AccessControlAllowOrigin] = context.Request.Headers[Header.Origin];
                context.Response.Headers[Header.AccessControlExposeHeaders] = String.Join(", ", context.Response.Headers.Keys);
                context.Response.Headers[Header.AccessControlAllowHeaders] = "Content-Type, Content-Length, Accept, Accept-Language, Accept-Charser, Accept-Encoding, Accept-Ranges, Authorization, X-Auth-Token";
            }

            var requestInfo = new RequestInfo(
                Verb.Parse(context.Request.Method),
                new Uri(context.Request.Uri.AbsoluteUri.TrimEnd('/')),
                context.Request.Body,
                new OwinPrincipal(context.Authentication.User),
                headers);
            var response = await _requestHandler.HandleRequestAsync(requestInfo);
            if ((IsResponseNoMatchingRouteFoundException(response)) && (Next != null))
            {
                await Next.Invoke(context);
                return;
            }

            context.Response.StatusCode = (int)response.Status;
            foreach (var header in response.Headers)
            {
                switch (header.Name)
                {
                    case Header.ContentLength:
                        context.Response.ContentLength = response.Body.Length;
                        break;
                    default:
                        context.Response.Headers.Add(header.Name, header.Values.Select(headerValue => headerValue.ToString()).ToArray());
                        break;
                }
            }

            response.Body.CopyTo(context.Response.Body);
        }

        private async Task HandleEmbeddedResource(IOwinContext context, string fileName, string mediaType)
        {
            context.Response.ContentType = mediaType;
            await HandleCors(context);
            using (var source = typeof(DescriptionController).Assembly.GetManifestResourceStream(fileName))
            {
                source.CopyTo(context.Response.Body);
            }
        }

        private async Task HandleCors(IOwinContext context)
        {
            await Task.Run(() =>
                {
                    if (!String.IsNullOrEmpty(context.Request.Headers[Header.Origin]))
                    {
                        context.Response.Headers[Header.AccessControlAllowOrigin] = context.Request.Headers[Header.Origin];
                    }
                });
        }
    }
}