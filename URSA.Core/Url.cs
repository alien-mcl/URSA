using System;

namespace URSA.Web.Http
{
    /// <summary>Provides an easy to use Url representation.</summary>
#if !CORE
    [Serializable]
#endif
    public abstract class Url
    {
        /// <summary>Gets the scheme of the Url.</summary>
        public abstract string Scheme { get; }

        /// <summary>Gets the location or scheme specific part of the Url.</summary>
        public abstract string Location { get; }

        /// <summary>Gets the host of the Url.</summary>
        public abstract string Host { get; }

        /// <summary>Gets the port of the Url.</summary>
        public abstract ushort Port { get; }

        /// <summary>Gets the original Url passed to the constructor.</summary>
        public abstract string OriginalUrl { get; }

        /// <summary>Performs an explicit conversion from <see cref="Uri"/> to <see cref="Url"/>.</summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Url(Uri uri)
        {
            return (uri == null ? null : UrlParser.Parse(uri.ToString()));
        }

        /// <summary>Performs an explicit conversion from <see cref="Url"/> to <see cref="Uri"/>.</summary>
        /// <param name="url">The URL.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator Uri(Url url)
        {
            if (url == null)
            {
                return null;
            }

            if (url.Scheme.Length == 0)
            {
                return new Uri(url.ToString(), UriKind.Relative);
            }

            return new Uri(url.ToString());
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return OriginalUrl;
        }
    }
}