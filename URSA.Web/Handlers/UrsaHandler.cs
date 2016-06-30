using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Routing;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Security;

namespace URSA.Web.Handlers
{
    /// <summary>Provides a connection between URSA framework and standard ASP.net pipeline.</summary>
    /// <typeparam name="T">Type of controller exposed.</typeparam>
    [ExcludeFromCodeCoverage]
    public class UrsaHandler<T> : IHttpHandler, IRouteHandler
        where T : IController
    {
        private readonly IRequestHandler<RequestInfo, ResponseInfo> _requestHandler;

        /// <summary>Initializes a new instance of the <see cref="UrsaHandler{T}" /> class.</summary>
        /// <param name="requestHandler">Request handler.</param>
        public UrsaHandler(IRequestHandler<RequestInfo, ResponseInfo> requestHandler)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException("requestHandler");
            }

            _requestHandler = requestHandler;
        }

        /// <inheritdoc />
        public bool IsReusable { get { return true; } }

        /// <inheritdoc />
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }

        /// <inheritdoc />
        public void ProcessRequest(HttpContext context)
        {
            context.Response.TrySkipIisCustomErrors = true;
            if (context.Request.Url.Segments.Any())
            {
                var segment = context.Request.Url.Segments.Last();
                switch (segment)
                {
                    case EntityConverter.DocumentationStylesheet:
                        HandleEmbeddedResource(context, "URSA.Web.Http.Description.DocumentationStylesheet.xslt", "text/xsl");
                        return;
                    case EntityConverter.PropertyIcon:
                        HandleEmbeddedResource(context, "URSA.Web.Http.Description.Property.png", "image/png");
                        return;
                    case EntityConverter.MethodIcon:
                        HandleEmbeddedResource(context, "URSA.Web.Http.Description.Method.png", "image/png");
                        return;
                }
            }

            HandleRequest(context);
        }

        private static void HandleEmbeddedResource(HttpContext context, string fileName, string mediaType)
        {
            context.Response.ContentType = mediaType;
            if (!String.IsNullOrEmpty(context.Request.Headers[Header.Origin]))
            {
                context.Response.Headers[Header.AccessControlAllowOrigin] = context.Request.Headers[Header.Origin];
            }

            using (var source = new StreamReader(typeof(DescriptionController).Assembly.GetManifestResourceStream(fileName)))
            {
                context.Response.Output.Write(source.ReadToEnd());
            }
        }

        private void HandleRequest(HttpContext context)
        {
            var headers = new HeaderCollection();
            context.Request.Headers.ForEach(headerName => ((IDictionary<string, string>)headers)[(string)headerName] = context.Request.Headers[(string)headerName]);
            var requestInfo = new RequestInfo(
                Verb.Parse(context.Request.HttpMethod),
                (HttpUrl)UrlParser.Parse(context.Request.Url.AbsoluteUri.TrimEnd('/')),
                context.Request.InputStream,
                new HttpContextPrincipal(context.User),
                headers);
            ResponseInfo response = _requestHandler.HandleRequest(requestInfo);
            context.Response.ContentEncoding = context.Response.HeaderEncoding = response.Encoding;
            context.Response.StatusCode = (int)response.Status;
            foreach (var header in response.Headers)
            {
                switch (header.Name)
                {
                    case Header.ContentType:
                        context.Response.ContentType = header.Value;
                        break;
                    case Header.ContentLength:
                        break;
                    default:
                        context.Response.Headers.Add(header.Name, header.Value);
                        break;
                }
            }

            response.Body.CopyTo(context.Response.OutputStream);
        }
    }
}