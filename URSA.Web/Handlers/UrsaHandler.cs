using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Routing;
using URSA.Security;
using URSA.Web.Http;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;
using URSA.Web.Http.Security;
using URSA.Web.Security;

namespace URSA.Web.Handlers
{
    /// <summary>Provides a connection between URSA framework and standard ASP.net pipeline.</summary>
    /// <typeparam name="T">Type of controller exposed.</typeparam>
    public class UrsaHandler<T> : IHttpHandler, IRouteHandler
        where T : IController
    {
        private readonly IRequestHandler<RequestInfo, ResponseInfo> _requestHandler;
        private readonly IEnumerable<IAuthenticationProvider> _authenticationProviders;

        /// <summary>Initializes a new instance of the <see cref="UrsaHandler{T}" /> class.</summary>
        /// <param name="requestHandler">Request handler.</param>
        /// <param name="authenticationProviders">Authentication providers.</param>
        public UrsaHandler(IRequestHandler<RequestInfo, ResponseInfo> requestHandler, IEnumerable<IAuthenticationProvider> authenticationProviders)
        {
            if (requestHandler == null)
            {
                throw new ArgumentNullException("requestHandler");
            }

            _requestHandler = requestHandler;
            _authenticationProviders = authenticationProviders ?? new IAuthenticationProvider[0];
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
                        HandleDocumentationStylesheet(context);
                        return;
                    case EntityConverter.PropertyIcon:
                        HandleIcon(context, "Property");
                        return;
                    case EntityConverter.MethodIcon:
                        HandleIcon(context, "Method");
                        return;
                }
            }

            HandleRequest(context);
        }

        private void HandleRequest(HttpContext context)
        {
            var requestInfo = AuthenticateRequest(context);
            var response = _requestHandler.HandleRequest(requestInfo);
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

        private HeaderCollection ParseHeaders(HttpContext context)
        {
            var result = new HeaderCollection();
            context.Request.Headers.ForEach(headerName => ((IDictionary<string, string>)result)[(string)headerName] = context.Request.Headers[(string)headerName]);
            return result;
        }

        private RequestInfo AuthenticateRequest(HttpContext context)
        {
            var requestInfo = new RequestInfo(
                Verb.Parse(context.Request.HttpMethod),
                new Uri(context.Request.Url.AbsoluteUri.TrimEnd('/')),
                context.Request.InputStream,
                new HttpContextPrincipal(context.User),
                ParseHeaders(context));
            var authorization = requestInfo.Headers.Authorization;
            int indexOf;
            if ((String.IsNullOrEmpty(authorization)) || ((indexOf = authorization.IndexOf(' ')) == -1))
            {
                return requestInfo;
            }

            var authenticationProvider = (from provider in _authenticationProviders
                                          where provider.Scheme == authorization.Substring(0, indexOf)
                                          select provider).FirstOrDefault();
            if (authenticationProvider != null)
            {
                authenticationProvider.Authenticate(requestInfo);
            }

            return requestInfo;
        }

        private void HandleDocumentationStylesheet(HttpContext context)
        {
            context.Response.ContentType = "text/xsl";
            HandleCors(context);
            using (var source = new StreamReader(typeof(DescriptionController).Assembly.GetManifestResourceStream("URSA.Web.Http.Description.DocumentationStylesheet.xslt")))
            {
                context.Response.Output.Write(source.ReadToEnd());
            }
        }

        private void HandleIcon(HttpContext context, string imagePath)
        {
            context.Response.ContentType = "image/png";
            HandleCors(context);
            using (var stream = typeof(DescriptionController).Assembly.GetManifestResourceStream(String.Format("URSA.Web.Http.Description.{0}.png", imagePath)))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                context.Response.BinaryWrite(buffer);
            }
        }

        private void HandleCors(HttpContext context)
        {
            if (!String.IsNullOrEmpty(context.Request.Headers[Header.Origin]))
            {
                context.Response.Headers[Header.AccessControlAllowOrigin] = context.Request.Headers[Header.Origin];
            }
        }
    }
}