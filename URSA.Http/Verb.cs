using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace URSA.Web.Http
{
    /// <summary>Represents an HTTP method verb.</summary>
    public class Verb
    {
        /// <summary>Represents an HTTP OPTIONS method.</summary>
        public static readonly Verb OPTIONS = new Verb("OPTIONS");

        /// <summary>Represents an HTTP HEAD method.</summary>
        public static readonly Verb HEAD = new Verb("HEAD");

        /// <summary>Represents an HTTP GET method.</summary>
        public static readonly Verb GET = new Verb("GET");

        /// <summary>Represents an HTTP PUT method.</summary>
        public static readonly Verb PUT = new Verb("PUT");

        /// <summary>Represents an HTTP POST method.</summary>
        public static readonly Verb POST = new Verb("POST");

        /// <summary>Represents an HTTP DELETE method.</summary>
        public static readonly Verb DELETE = new Verb("DELETE");

        /// <summary>Defines a collection of all HTTP verbs.</summary>
        public static readonly IEnumerable<Verb> Verbs = new[] { OPTIONS, HEAD, GET, PUT, POST, DELETE };

        private readonly string _verb;

        /// <summary>Initializes a new instance of the <see cref="Verb"/> class.</summary>
        /// <param name="verb">The verb.</param>
        [ExcludeFromCodeCoverage]
        public Verb(string verb)
        {
            if (verb == null)
            {
                throw new ArgumentNullException("verb");
            }

            _verb = verb;
        }

        /// <summary>Parses a given string into an instance of the <see cref="Verb" /> class.</summary>
        /// <param name="detectedVerb">Verb string to be parsed.</param>
        /// <returns>Instance of the <see cref="Verb" /> class if the parsing was successful; otherwise <b>null</b>.</returns>
        public static Verb Parse(string detectedVerb)
        {
            if (detectedVerb == null)
            {
                throw new ArgumentNullException("detectedVerb");
            }

            if (detectedVerb.Length == 0)
            {
                throw new ArgumentOutOfRangeException("detectedVerb");
            }

            return (from field in typeof(Verb).GetFields(BindingFlags.Public | BindingFlags.Static)
                    where (field.IsInitOnly) && (field.FieldType == typeof(Verb)) && 
                        (StringComparer.OrdinalIgnoreCase.Equals(field.Name, detectedVerb))
                    select (Verb)field.GetValue(null)).FirstOrDefault();
        }

        /// <summary>Checks for equality of two <see cref="Verb" />s.</summary>
        /// <param name="operandA">Left operand.</param>
        /// <param name="operandB">Right operand.</param>
        /// <returns><b>true</b> if both operands are <b>null</b> or both represents the same verb; otherwise <b>false</b>.</returns>
        public static bool operator ==(Verb operandA, Verb operandB)
        {
            return ((Object.Equals(operandA, null)) && (Object.Equals(operandB, null))) ||
                ((!Object.Equals(operandA, null)) && (!Object.Equals(operandB, null)) && 
                (operandA._verb.GetHashCode() == operandB._verb.GetHashCode()));
        }

        /// <summary>Checks for inequality of two <see cref="Verb" />s.</summary>
        /// <param name="operandA">Left operand.</param>
        /// <param name="operandB">Right operand.</param>
        /// <returns><b>false</b> if both operands are <b>null</b> or both represents the same verb; otherwise <b>true</b>.</returns>
        public static bool operator !=(Verb operandA, Verb operandB)
        {
            return ((Object.Equals(operandA, null)) && (!Object.Equals(operandB, null))) ||
                ((!Object.Equals(operandA, null)) && (Object.Equals(operandB, null))) ||
                ((!Object.Equals(operandA, null)) && (!Object.Equals(operandB, null)) && (operandA._verb.GetHashCode() != operandB._verb.GetHashCode()));
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return _verb;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            return _verb.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if ((Object.Equals(obj, null)) || (!(obj is Verb)))
            {
                return false;
            }

            Verb method = (Verb)obj;
            return _verb.Equals(method._verb);
        }
    }
}