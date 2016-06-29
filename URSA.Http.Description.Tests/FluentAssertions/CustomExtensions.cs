﻿using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using VDS.RDF;

namespace URSA.Http.Description.Tests.FluentAssertions
{
    public static class CustomExtensions
    {
        /// <summary>Determines whether the subject is an equivalent to <paramref name="anotherGraph" />.</summary>
        /// <param name="subject">Subject graph.</param>
        /// <param name="anotherGraph">Another graph.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="reasonArgs">Zero or more objects to format using the placeholders in <paramref name="because" />.</param>
        /// <returns>Entry point for further assetions.</returns>
        public static AndWhichConstraint<ObjectAssertions, Graph> AndBeEquivalent(this AndWhichConstraint<ObjectAssertions, Graph> subject, Graph anotherGraph, string because = "", params object[] reasonArgs)
        {
            var sameStatements = (from sourceTriple in subject.Which.Triples
                                  from targetTriple in anotherGraph.Triples
                                  where sourceTriple.Subject.Equals(targetTriple.Subject) &&
                                      sourceTriple.Predicate.Equals(targetTriple.Predicate) &&
                                      sourceTriple.Object.Equals(targetTriple.Object)
                                  select 1).Sum();
            Execute.Assertion
                .ForCondition(sameStatements == subject.Which.Triples.Count)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected both graphs to have same statements, but {0} differed.", Math.Abs(subject.Which.Triples.Count - sameStatements));
            return subject;
        }
    }
}