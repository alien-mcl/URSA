using System;
using System.Linq;
using FluentAssertions.Common;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using URSA.Web.Http;
using URSA.Web.Http.Tests.Testing;

namespace FluentAssertions
{
    /// <summary>Exposes useful <see cref="HttpUrl" /> assertions.</summary>
    public class HttpUrlAssertions : ReferenceTypeAssertions<HttpUrl, HttpUrlAssertions>
    {
        /// <summary>Initializes a new instance of the <see cref="HttpUrlAssertions"/> class.</summary>
        /// <param name="url">The subject URL.</param>
        public HttpUrlAssertions(HttpUrl url)
        {
            Subject = url;
        }

        /// <inheritdoc />
        protected override string Context { get { return "URSA.Web.Http.HttpUrl"; } }

        /// <summary>Checks whether the subject <see cref="Url" /> complies with a test scenario.</summary>
        /// <param name="scenario">The scenario to check against.</param>
        /// <param name="because">Justification test.</param>
        /// <param name="reasonArgs">Justification text arguments.</param>
        /// <returns>Further <see cref="Url" /> assertions.</returns>
        public AndConstraint<HttpUrlAssertions> ComplyWith(UrlScenario scenario, string because = "", params object[] reasonArgs)
        { 
            if (Subject == null)
            {
                return new AndConstraint<HttpUrlAssertions>(this);
            }

            if (scenario.AsString.Length > 0)
            {
                Execute.Assertion
                    .ForCondition(Subject.ToString() == scenario.AsString)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{2}> string representation to be {0}, but found {1}.", scenario.AsString, Subject.ToString(), Subject.OriginalUrl);
            }

            if (scenario.Scheme.Length > 0)
            {
                Execute.Assertion
                    .ForCondition(Subject.Scheme == scenario.Scheme)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{2}> scheme to be {0}, but found {1}.", scenario.Scheme, Subject.Scheme, Subject.OriginalUrl);
            }

            if (scenario.Host.Length > 0)
            {
                Execute.Assertion
                    .ForCondition(Subject.Host == scenario.Host)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{2}> host to be {0}, but found {1}.", scenario.Host, Subject.Host, Subject.OriginalUrl);
            }

            if (scenario.Path.Length > 0)
            {
                Execute.Assertion
                    .ForCondition(Subject.Path == scenario.Path)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{2}> path to be {0}, but found {1}.", scenario.Path, Subject.Path, Subject.OriginalUrl);
            }

            if (scenario.Parameters != null)
            {
                Execute.Assertion
                    .ForCondition(Subject.HasQuery)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{0}> to have a query, but it didn't have one.", Subject.OriginalUrl);

                Execute.Assertion
                    .ForCondition(Subject.Query.All(entry => scenario.Parameters.Any(item => (entry.Key == item.Key) && (entry.Value == item.Value))))
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{2}> query to be {0}, but found {1}.", String.Join("&", scenario.Parameters.Select(item => String.Format("{0}={1}", item.Key, item.Value))), Subject.Query, Subject.OriginalUrl);
            }
            else
            {
                Execute.Assertion
                    .ForCondition(!Subject.HasQuery)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{0}> not to have a query, but it had one.", Subject.OriginalUrl);
            }

            if (scenario.Fragment != null)
            {
                Execute.Assertion
                    .ForCondition(Subject.HasFragment)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{0}> to have a fragment, but it didn't have one.", Subject.OriginalUrl);

                Execute.Assertion
                    .ForCondition(Subject.Fragment == scenario.Fragment)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{2}> fragment to be {0}, but found {1}.", scenario.Fragment, Subject.Fragment, Subject.OriginalUrl);
            }
            else
            {
                Execute.Assertion
                    .ForCondition(!Subject.HasFragment)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{0}> not to have a fragment, but it had one.", Subject.OriginalUrl);
            }

            if (scenario.Port > 0)
            {
                Execute.Assertion
                    .ForCondition(Subject.Port == scenario.Port)
                    .BecauseOf(because, reasonArgs)
                    .FailWith("Expected Url's <{2}> port to be {0}, but found {1}.", scenario.Port, Subject.Port, Subject.OriginalUrl);
            }

            return new AndConstraint<HttpUrlAssertions>(this);
        }

        /// <summary>Checks whether the subject of the assertions equals to <paramref name="expected" /> URL.</summary>
        /// <param name="other">The other URL.</param>
        /// <param name="because">Justification string.</param>
        /// <param name="reasonArgs">Justification arguments.</param>
        /// <returns>Further <see cref="Url" /> assertions.</returns>
        public AndConstraint<HttpUrlAssertions> Be(HttpUrl expected, string because = "", params object[] reasonArgs)
        {
            Execute.Assertion.BecauseOf(because, reasonArgs)
                .ForCondition(Subject.IsSameOrEqualTo(expected))
                .FailWith("Expected {context:object} to be {0}{reason}, but found {1}.", expected, Subject);
            return new AndConstraint<HttpUrlAssertions>(this);
        }
    }
}