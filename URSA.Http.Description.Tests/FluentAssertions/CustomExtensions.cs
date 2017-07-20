using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace URSA.Http.Description.Tests.FluentAssertions
{
    public static class CustomExtensions
    {
        /// <summary>Tests a givent <paramref name="subject" /> string wether it equals to a string taken from the <paramref name="streamName" /> stream.</summary>
        /// <remarks>This method replaces all carriage-return/new-line-feeds to new-line-feeds.</remarks>
        /// <param name="subject">The subject.</param>
        /// <param name="streamName">Name of the stream.</param>
        /// <param name="because">The because.</param>
        /// <param name="reasonArgs">The reason arguments.</param>
        /// <returns>Additional assertion concatenated with the previous one with an AND operator.</returns>
        public static AndConstraint<StringAssertions> BeEquivalentToStream(this StringAssertions subject, string streamName, string because = "", params object[] reasonArgs)
        {
            var expected = new StreamReader(typeof(CustomExtensions).GetTypeInfo().Assembly.GetManifestResourceStream(streamName)).ReadToEnd().CleanupText();
            subject.Subject.CleanupText().Should().Be(expected, because, reasonArgs);
            return new AndConstraint<StringAssertions>(subject);
        }

        private static string CleanupText(this string text)
        {
            return Regex.Replace(Regex.Replace(Regex.Replace(text, "\r\n", "\n"), " {2,}", String.Empty), "\n{2,}", String.Empty);
        }
    }
}