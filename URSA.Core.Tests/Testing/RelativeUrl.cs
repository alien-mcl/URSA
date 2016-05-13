using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URSA.Web.Http;

namespace URSA.Testing
{
    public class RelativeUrl : Url
    {
        private readonly string _url;

        public RelativeUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            if ((url.Length == 0) || (!url.StartsWith("/")))
            {
                throw new ArgumentOutOfRangeException("url");
            }

            _url = url;
        }

        public override string Scheme { get { return String.Empty; } }

        public override string Location { get { return _url; } }

        public override string Host { get { return String.Empty; } }

        public override ushort Port { get { return 80; } }

        public override string OriginalUrl { get { return _url; } }

        public override bool Equals(object obj)
        {
            RelativeUrl anotherUrl = obj as RelativeUrl;
            return (anotherUrl != null) && (_url.Equals(anotherUrl._url));
        }

        public override int GetHashCode()
        {
            return _url.GetHashCode();
        }

        public override string ToString()
        {
            return _url;
        }
    }
}