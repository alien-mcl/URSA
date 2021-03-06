﻿using System.Text;
using System.Web;

namespace System
{
    /// <summary>Provides useful <see cref="Uri" /> extension methods.</summary>
    public static class UriExtensions
    {
        /// <summary>Converts a given uri to a relative one.</summary>
        /// <param name="uri">Uri to be converted.</param>
        /// <returns>Relative <see cref="Uri" /> if the given <paramref name="uri" /> is absolute; otherwise <paramref name="uri" />.</returns>
        public static Uri ToRelativeUri(this Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (!uri.IsAbsoluteUri)
            {
                return uri;
            }

            string result = uri.ToString();
            result = result.Substring(uri.ToString().IndexOf(uri.Host) + uri.Host.Length);
            if (result[0] == ':')
            {
                result = result.Substring(1 + uri.Port.ToString().Length);
            }

            return new Uri(result, UriKind.Relative);
        }

        /// <summary>Combines two <see cref="Uri" />s altogether, without checking if the base one is an absolute uri.</summary>
        /// <param name="uri">Relative uri to be appended.</param>
        /// <param name="baseUri">Base uri.</param>
        /// <returns><see cref="Uri" /> being a combination of the <paramref name="baseUri" /> and <paramref name="uri" />.</returns>
        public static Uri Combine(this Uri uri, Uri baseUri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (baseUri == null)
            {
                throw new ArgumentNullException("baseUri");
            }

            if (baseUri.IsAbsoluteUri)
            {
                Uri relativeUri = uri;
                if (relativeUri.ToString().StartsWith("/"))
                {
                    if (!baseUri.ToString().EndsWith("/"))
                    {
                        baseUri = new Uri(baseUri.ToString() + "/");
                    }

                    relativeUri = new Uri(relativeUri.ToString().Substring(1), UriKind.Relative);
                }

                return new Uri(baseUri, relativeUri);
            }

            string uriString = uri.ToString();
            if (uriString == "/")
            {
                return baseUri;
            }

            string baseUriString = VirtualPathUtility.AppendTrailingSlash(baseUri.ToString());
            string result = VirtualPathUtility.Combine(
                (baseUriString[0] == '/' ? String.Empty : "/") + baseUriString,
                (uriString[0] == '/' ? uriString.Substring(1) : uriString));
            return new Uri(result, UriKind.Relative);
        }

        /// <summary>Adds a fragment to given uri.</summary>
        /// <remarks>If the uri already has a fragment, it will be converted to segment.</remarks>
        /// <param name="uri">Uri to add fragment to.</param>
        /// <param name="fragment">Fragment to be added.</param>
        /// <returns><see cref="Uri" /> with fragment added.</returns>
        public static Uri AddFragment(this Uri uri, string fragment)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (uri.Scheme == "urn")
            {
                return AddName(uri, fragment);
            }

            if (fragment != null)
            {
                var iri = new StringBuilder(uri.ToString());
                if (uri.Fragment.Length > 0)
                {
                    int hash = iri.ToString().LastIndexOf('#');
                    iri.Remove(hash, 1);
                    if (iri[hash - 1] != '/')
                    {
                        iri.Insert(hash, '/');
                    }
                }
                else if ((iri[iri.Length - 1] != '/') && (uri.Query.Length == 0))
                {
                    iri.Append('/');
                }

                iri.Append("#");
                iri.Append(fragment);
                return new Uri(iri.ToString());
            }

            return uri;
        }

        /// <summary>Adds a name to given URN.</summary>
        /// <remarks>Appended name will start with dot (.).</remarks>
        /// <param name="uri">Uri to add name to.</param>
        /// <param name="name">Name to be added.</param>
        /// <returns><see cref="Uri" /> with name added.</returns>
        public static Uri AddName(this Uri uri, string name)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (!String.IsNullOrEmpty(name))
            {
                return new Uri(uri.ToString() + "." + name);
            }
            else
            {
                return uri;
            }
        }
    }
}