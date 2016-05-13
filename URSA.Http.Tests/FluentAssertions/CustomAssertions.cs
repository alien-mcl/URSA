using FluentAssertions.Primitives;
using URSA.Web.Http;

namespace FluentAssertions
{
    /// <summary>Provides custom fluent assertions.</summary>
    public static class CustomAssertions
    {
        /// <summary> Asserts that the object is not of the specified type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T"> The type that the subject is not supposed to be of.</typeparam>
        /// <param name="assertions">Parent assertions.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="reasonArgs">Zero or more objects to format using the placeholders in <see cref="because" />.</param>
        public static AndConstraint<ObjectAssertions> NotBeOfType<T>(this ObjectAssertions assertions, string because = "", params object[] reasonArgs)
        {
            if (assertions.Subject != null)
            {
                assertions.Subject.GetType().Should().NotBe(typeof(T), because, reasonArgs);
            }

            return new AndConstraint<ObjectAssertions>(assertions);
        }

        /// <summary>Exposes the <see cref="System.Security.Policy.Url" /> assertions extension.</summary>
        /// <param name="url">The subject URL.</param>
        /// <returns>Assertions for a given <paramref name="url" />.</returns>
        public static HttpUrlAssertions Should(this HttpUrl url)
        {
            return new HttpUrlAssertions(url);
        }

        /// <summary>Exposes the <see cref="System.Security.Policy.Url" /> assertions extension.</summary>
        /// <param name="url">The subject URL.</param>
        /// <returns>Assertions for a given <paramref name="url" />.</returns>
        public static FtpUrlAssertions Should(this FtpUrl url)
        {
            return new FtpUrlAssertions(url);
        }
    }
}