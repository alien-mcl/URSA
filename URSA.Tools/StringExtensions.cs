using System.Text;
using System.Text.RegularExpressions;

namespace System
{
    /// <summary>Provides useful string extension methods.</summary>
    public static class StringExtensions
    {
        /// <summary>Converts a camelCase or PascalCase strings to corresponding camel Case or Pascal Case strings.</summary>
        /// <param name="pascalOrCamelCaseString">String to be converted.</param>
        /// <returns>Converted string with spaces.</returns>
        public static string ToDisplayString(this string pascalOrCamelCaseString)
        {
            string result = pascalOrCamelCaseString;
            if (result != null)
            {
                result = Regex.Replace(
                    pascalOrCamelCaseString,
                    "([a-z])([A-Z])",
                    match => String.Format("{0} {1}", match.Groups[0], match.Groups[1]));
            }

            return result;
        }

        /// <summary>Indents the specified text.</summary>
        /// <param name="text">The text to be indented.</param>
        /// <param name="indentation">The indentation.</param>
        /// <returns>Text with indented new lines.</returns>
        public static string Indent(this string text, int indentation)
        {
            string result = text;
            if ((indentation <= 0) || (result == null))
            {
                return result;
            }

            return result.Replace(Environment.NewLine, Environment.NewLine + new String(' ', indentation));
        }

        /// <summary>Converts a given text value into a lowerCamelCase string.</summary>
        /// <param name="value">Value to be converter</param>
        /// <returns>Text converted to lowerCamelCase or <b>null</b> if the input <paramref name="value" /> was also <b>null</b>.</returns>
        public static string ToLowerCamelCase(this string value)
        {
            return value.ToCamelCase(true);
        }

        /// <summary>Converts a given text value into a UpperCamelCase string (aka. Pascal case).</summary>
        /// <param name="value">Value to be converter</param>
        /// <returns>Text converted to UpperCamelCase or <b>null</b> if the input <paramref name="value" /> was also <b>null</b>.</returns>
        public static string ToUpperCamelCase(this string value)
        {
            return value.ToCamelCase(false);
        }

        private static string ToCamelCase(this string value, bool lowerCase)
        {
            string result = value;
            if (!String.IsNullOrEmpty(result))
            {
                result = Regex.Replace(Regex.Replace(value, "([a-zA-Z])(\\s+)([a-zA-Z])", ReplaceSpace), "\\W", "_");
                result = (lowerCase ? Char.ToLower(value[0]) : Char.ToUpper(value[0])) + value.Substring(1);
            }

            return result;
        }

        private static string ReplaceSpace(Match match)
        {
            bool isFirstLetter = false;
            StringBuilder result = new StringBuilder(match.Value.Length - 1);
            for (int index = 2; index < match.Groups.Count; index++)
            {
                string value = match.Groups[index].Value.Trim();
                if (value.Length > 0)
                {
                    result.Append((!isFirstLetter) && (isFirstLetter = !isFirstLetter) ? value.ToLower() : value.ToUpper());
                }
            }

            return result.ToString();
        }
    }
}