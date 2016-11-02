using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#if CORE
using URSA.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
#else
using Microsoft.Owin;
#endif
using URSA.Owin.Security;
using URSA.Web;
using URSA.Web.Http;
using URSA.Web.Http.Configuration;
using URSA.Web.Http.Converters;
using URSA.Web.Http.Description;

namespace URSA.Owin.Handlers
{
    /// <summary>Provides a connection between URSA framework and Owin pipeline.</summary>
    [ExcludeFromCodeCoverage]
    public class UrsaHandler
    {
        private readonly IRequestHandler<RequestInfo, ResponseInfo> _requestHandler;

        /// <summary>Initializes a new instance of the <see cref="UrsaHandler"/> class.</summary>
        /// <param name="next">Next middleware in the pipeline.</param>
        /// <param name="requestHandler">The request handler.</param>
        public UrsaHandler(Func<Task> next, IRequestHandler<RequestInfo, ResponseInfo> requestHandler)
        {
            Next = next;
            if (requestHandler == null)
            {
                throw new ArgumentNullException("requestHandler");
            }

            _requestHandler = requestHandler;
        }

        private Func<Task> Next { get; set; }

        /// <summary>Invokes the handler.</summary>
        /// <param name="context">HTTP request context.</param>
        /// <returns>Task of the invokation.</returns>
        public async Task Invoke(
#if CORE
            HttpContext context)
#else
            IOwinContext context)
#endif
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if ((context.Request == null) || (context.Response == null))
            {
                throw new ArgumentOutOfRangeException("context");
            }

            await ProcessRequest(context);
        }

#if CORE
        private static RequestInfo CreateRequestInfo(HttpContext context, HeaderCollection headers)
        {
            return new RequestInfo(
                Verb.Parse(context.Request.Method),
                (HttpUrl)UrlParser.Parse(context.Request.ToUrlString()),
                context.Request.Body,
                new OwinPrincipal(context.User),
                headers);
        }
#else
        private static RequestInfo CreateRequestInfo(IOwinContext context, HeaderCollection headers)
        {
            return new RequestInfo(
                Verb.Parse(context.Request.Method),
                (HttpUrl)UrlParser.Parse(context.Request.Uri.AbsoluteUri.TrimEnd('/')),
                context.Request.Body,
                new OwinPrincipal(context.Authentication.User),
                headers);
        }
#endif

        private static bool IsResponseNoMatchingRouteFoundException(ResponseInfo response)
        {
            return ((response.Status == HttpStatusCode.NotFound) && (response is ExceptionResponseInfo) &&
                (((ExceptionResponseInfo)response).Value is NoMatchingRouteFoundException));
        }

        private static async Task HandleEmbeddedResource(
#if CORE
            HttpContext context,
#else
            IOwinContext context,
#endif
            string fileName,
            string mediaType)
        {
            context.Response.ContentType = mediaType;
            if (!String.IsNullOrEmpty(context.Request.Headers[Header.Origin]))
            {
                context.Response.Headers[Header.AccessControlAllowOrigin] = context.Request.Headers[Header.Origin];
            }

            using (var source = typeof(DescriptionController).GetTypeInfo().Assembly.GetManifestResourceStream(fileName))
            {
                await source.CopyToAsync(context.Response.Body);
            }
        }

        private async Task ProcessRequest(
#if CORE
            HttpContext context)
#else
            IOwinContext context)
#endif
        {
#if CORE
            var segments = context.Request.Path.Value.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
#else
            var segments = context.Request.Uri.Segments;
#endif
            if (segments.Any())
            {
                var segment = segments.Last();
                var resources = from resourceName in typeof(DescriptionController).GetTypeInfo().Assembly.GetManifestResourceNames() select resourceName;
                switch (segment)
                {
                    case EntityConverter.DocumentationStylesheet:
                        await HandleEmbeddedResource(context, resources.FirstOrDefault(resourceName => resourceName.IndexOf("DocumentationStylesheet.xslt") != -1), "text/xsl");
                        return;
                    case EntityConverter.PropertyIcon:
                        await HandleEmbeddedResource(context, resources.FirstOrDefault(resourceName => resourceName.IndexOf("Property.png") != -1), "image/png");
                        return;
                    case EntityConverter.MethodIcon:
                        await HandleEmbeddedResource(context, resources.FirstOrDefault(resourceName => resourceName.IndexOf("Method.png") != -1), "image/png");
                        return;
                }
            }

            await HandleRequest(context);
        }

        private async Task HandleRequest(
#if CORE
            HttpContext context)
#else
            IOwinContext context)
#endif
        {
            var headers = new HeaderCollection();
            context.Request.Headers.ForEach(header => ((IDictionary<string, string>)headers)[header.Key] = String.Join(",", header.Value));
            var requestInfo = CreateRequestInfo(context, headers);
            ResponseInfo response = await _requestHandler.HandleRequestAsync(requestInfo);
            if ((IsResponseNoMatchingRouteFoundException(response)) && (Next != null))
            {
                await Next();
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
    }
}