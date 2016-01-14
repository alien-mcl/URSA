using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace URSA.Web.Http
{
    /// <content>Defines known header properties.</content>
    public sealed partial class HeaderCollection
    {
        /// <summary>Gets or sets the 'Access-Control-Request-Method' header's value.</summary>
        public string AccessControlRequestMethod
        {
            get { return (this[Http.Header.AccessControlRequestMethod] != null ? String.Join(",", this[Http.Header.AccessControlRequestMethod].Values) : null); }
            set { Set(Http.Header.AccessControlRequestMethod, value); }
        }

        /// <summary>Gets or sets the 'Access-Control-Allow-Origin' header's value.</summary>
        public string AccessControlAllowOrigin
        {
            get { return (this[Http.Header.AccessControlAllowOrigin] != null ? String.Join(",", this[Http.Header.AccessControlAllowOrigin].Values) : null); }
            set { Set(Http.Header.AccessControlAllowOrigin, value); }
        }

        /// <summary>Gets or sets the 'Access-Control-Expose-Headers' header's value.</summary>
        public string AccessControlExposeHeaders
        {
            get { return (this[Http.Header.AccessControlExposeHeaders] != null ? String.Join(",", this[Http.Header.AccessControlExposeHeaders].Values) : null); }
            set { Set(Http.Header.AccessControlExposeHeaders, value); }
        }

        /// <summary>Gets or sets the 'Access-Control-Allow-Headers' header's value.</summary>
        public string AccessControlAllowHeaders
        {
            get { return (this[Http.Header.AccessControlAllowHeaders] != null ? String.Join(",", this[Http.Header.AccessControlAllowHeaders].Values) : null); }
            set { Set(Http.Header.AccessControlAllowHeaders, value); }
        }

        /// <summary>Gets or sets the 'Authorization' header's value.</summary>
        public string Authorization
        {
            get { return (this[Http.Header.Authorization] != null ? String.Join(",", this[Http.Header.Authorization].Values) : null); }
            set { Set(Http.Header.Authorization, value); }
        }

        /// <summary>Gets or sets the 'Content-Length' header's value.</summary>
        /// <remarks>If the header does not exist, value of 0 is returned.</remarks>
        public int ContentLength
        {
            get
            {
                return (this[Http.Header.ContentLength] != null ? ((Header<int>)this[Http.Header.ContentLength]).Values.First().Value : default(int));
            }

            set
            {
                Header<int> contentLength = (Header<int>)this[Http.Header.ContentLength];
                if (contentLength == null)
                {
                    this[Http.Header.ContentLength] = new Header<int>(Http.Header.ContentLength, value);
                }
                else
                {
                    ((Header<int>)this[Http.Header.ContentLength]).Values.First().Value = value;
                }
            }
        }

        /// <summary>Gets or sets the 'Content-Type' header's value.</summary>
        public string ContentType
        {
            get { return (this[Http.Header.ContentType] != null ? String.Join(",", this[Http.Header.ContentType].Values) : null); }
            set { Set(Http.Header.ContentType, value); }
        }

        /// <summary>Gets or sets the 'Accept' header's value.</summary>
        public string Accept
        {
            get { return (this[Http.Header.Accept] != null ? String.Join(",", this[Http.Header.Accept].Values) : null); }
            set { Set(Http.Header.Accept, value); }
        }

        /// <summary>Gets or sets the 'Content-Disposition' header's value.</summary>
        public string ContentDisposition
        {
            get { return (this[Http.Header.ContentDisposition] != null ? String.Join(",", this[Http.Header.ContentDisposition].Values) : null); }
            set { Set(Http.Header.ContentDisposition, value); }
        }

        /// <summary>Gets or sets the 'Origin' header's value.</summary>
        public string Origin
        {
            get { return (this[Http.Header.Origin] != null ? String.Join(",", this[Http.Header.Origin].Values) : null); }
            set { Set(Http.Header.Origin, value); }
        }

        /// <summary>Gets or sets the 'Location' header's value.</summary>
        public string Location
        {
            get { return (this[Http.Header.Location] != null ? String.Join(",", this[Http.Header.Location].Values) : null); }
            set { Set(Http.Header.Location, value); }
        }

        /// <summary>Gets or sets the 'WWW-Authenticate' header's value.</summary>
        public string WWWAuthenticate
        {
            get { return (this[Http.Header.WWWAuthenticate] != null ? String.Join(",", this[Http.Header.WWWAuthenticate].Values) : null); }
            set { Set(Http.Header.WWWAuthenticate, value); }
        }

        /// <summary>Gets or sets the 'X-Requested-With' header's value.</summary>
        public string XRequestedWith
        {
            get { return (this[Http.Header.XRequestedWith] != null ? String.Join(",", this[Http.Header.XRequestedWith].Values) : null); }
            set { Set(Http.Header.XRequestedWith, value); }
        }
    }
}