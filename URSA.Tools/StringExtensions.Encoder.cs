using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

namespace System
{
    /// <content>Contains a JavaScript encoding routines.</content>
    /// <remarks>This code is based on Mono implementation of the System.Web.HttpUtility class.</remarks>
    public static partial class StringExtensions
    {
        private static readonly char[] HexChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        private static readonly IDictionary<char, string> ReplacementMap = new Dictionary<char, string>()
            {
                { '\x08', "\\b" },
                { '\x09', "\\t" },
                { '\x0A', "\\n" },
                { '\x0C', "\\f" },
                { '\x0D', "\\r" },
                { '\x22', "\\\"" },
                { '\x5C', "\\\\" },
            };

        /// <summary>Decodes an Url encoded string.</summary>
        /// <param name="value">Input value.</param>
        /// <returns>Decoded string.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        public static string UrlDecode(this string value)
        {
            return value.UrlDecode(Encoding.UTF8);
        }

        /// <summary>Decodes an Url encoded string.</summary>
        /// <param name="value">Input value.</param>
        /// <param name="encoding">String encoding.</param>
        /// <returns>Decoded string.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        public static string UrlDecode(this string value, Encoding encoding)
        {
            if ((value == null) || ((value.IndexOf('%') == -1) && (value.IndexOf('+') == -1)))
            {
                return value;
            }

            encoding = encoding ?? Encoding.UTF8;
            var bytes = new List<byte>();
            for (int index = 0; index < value.Length; index++)
            {
                char currentChar = value[index];
                if ((currentChar == '%') && (index + 2 < value.Length) && (value[index + 1] != '%'))
                {
                    int extendedChar;
                    if ((value[index + 1] == 'u') && (index + 5 < value.Length))
                    {
                        extendedChar = value.GetChar(index + 2, 4);
                        if (extendedChar != -1)
                        {
                            bytes.WriteCharBytes((char)extendedChar, encoding);
                            index += 5;
                        }
                        else
                        {
                            bytes.WriteCharBytes('%', encoding);
                        }
                    }
                    else if ((extendedChar = value.GetChar(index + 1, 2)) != -1)
                    {
                        bytes.WriteCharBytes((char)extendedChar, encoding);
                        index += 2;
                    }
                    else
                    {
                        bytes.WriteCharBytes('%', encoding);
                    }

                    continue;
                }

                bytes.WriteCharBytes((currentChar == '+' ? ' ' : currentChar), encoding);
            }

            return encoding.GetString(bytes.ToArray());
        }

        /// <summary>Encodes a given <paramref name="value" /> into na Url safe string.</summary>
        /// <param name="value">Value to be encoded.</param>
        /// <returns>Encoded string.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        public static string UrlEncode(this string value)
        {
            return value.UrlEncode(Encoding.UTF8);
        }

        /// <summary>Encodes a given <paramref name="value" /> into na Url safe string.</summary>
        /// <param name="value">Value to be encoded.</param>
        /// <param name="encoding">String encoding.</param>
        /// <returns>Encoded string.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        public static string UrlEncode(this string value, Encoding encoding)
        {
            if ((System.String.IsNullOrEmpty(value)) || (value.Where(RequiresUrlProcessing).All(IsNotUrlEncoded)))
            {
                return value;
            }

            byte[] buffer = new byte[encoding.GetMaxByteCount(value.Length)];
            int length = encoding.GetBytes(value, 0, value.Length, buffer, 0);
            return Encoding.ASCII.GetString(UrlEncodeToBytes(buffer, 0, length));
        }

        /// <summary>Encodes a given <paramref name="value" /> to be a safe JavaScript string.</summary>
        /// <param name="value">Input string.</param>
        /// <returns>JavaScript escaped string or <b>null</b> in case the <paramref name="value" /> is also <b>null</b>.</returns>
        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        public static string JavaScriptStringEncode(this string value)
        {
            if ((String.IsNullOrEmpty(value)) || (!value.Any(RequiresJavaScriptProcessing)))
            {
                return value;
            }

            var result = new StringBuilder();
            foreach (var currentChar in value)
            {
                string replacement;
                if (currentChar.RequiresJavaScriptEscaping())
                {
                    result.AppendFormat("\\u{0:x4}", (int)currentChar);
                }
                else if (ReplacementMap.TryGetValue(currentChar, out replacement))
                {
                    result.Append(replacement);
                }
                else
                {
                    result.Append(currentChar);
                }
            }

            return result.ToString();
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static bool RequiresJavaScriptProcessing(this char currentChar)
        {
            return (currentChar >= 0) && ((currentChar <= 31) || (currentChar == 34) || (currentChar == 39) || (currentChar == 60) || (currentChar == 62) || (currentChar == 92));
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static bool RequiresJavaScriptEscaping(this char currentChar)
        {
            return (currentChar >= 0) && ((currentChar <= 7) || (currentChar == 11) || ((currentChar >= 14) && (currentChar <= 31)) || (currentChar == 39) || (currentChar == 60) || (currentChar == 62));
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
        {
            if (bytes.Length == 0)
            {
                return new byte[0];
            }

            MemoryStream result = new MemoryStream(count);
            int endIndex = offset + count;
            for (int index = offset; index < endIndex; index++)
            {
                ((char)bytes[index]).UrlEncodeChar(result);
            }

            return result.ToArray();
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static bool RequiresUrlProcessing(this char currentChar)
        {
            return (currentChar < '0') || (currentChar < 'A' && currentChar > '9') || (currentChar > 'Z' && currentChar < 'a') || (currentChar > 'z');
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static bool IsNotUrlEncoded(this char currentChar)
        {
            return (currentChar == '!') || (currentChar == '(') || (currentChar == ')') || (currentChar == '*') || (currentChar == '-') || (currentChar == '.') || (currentChar == '_');
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static void UrlEncodeChar(this char currentChar, Stream result)
        {
            int index;
            if (currentChar > 255)
            {
                int charValue = currentChar;
                result.WriteByte((byte)'%');
                result.WriteByte((byte)'u');
                index = charValue >> 12;
                result.WriteByte((byte)HexChars[index]);
                index = (charValue >> 8) & 0x0F;
                result.WriteByte((byte)HexChars[index]);
                index = (charValue >> 4) & 0x0F;
                result.WriteByte((byte)HexChars[index]);
                index = charValue & 0x0F;
                result.WriteByte((byte)HexChars[index]);
                return;
            }

            if ((currentChar > ' ') && (currentChar.IsNotUrlEncoded()))
            {
                result.WriteByte((byte)currentChar);
                return;
            }

            if (currentChar == ' ')
            {
                result.WriteByte((byte)'+');
                return;
            }

            if ((currentChar < '0') ||
                (currentChar < 'A' && currentChar > '9') ||
                (currentChar > 'Z' && currentChar < 'a') ||
                (currentChar > 'z'))
            {
                result.WriteByte((byte)'%');
                index = currentChar >> 4;
                result.WriteByte((byte)HexChars[index]);
                index = currentChar & 0x0F;
                result.WriteByte((byte)HexChars[index]);
            }
            else
            {
                result.WriteByte((byte)currentChar);
            }
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static int GetChar(this string value, int offset, int length)
        {
            int result = 0;
            int end = length + offset;
            for (int index = offset; index < end; index++)
            {
                char currentChar = value[index];
                if (currentChar > 127)
                {
                    return -1;
                }

                int current = ((byte)currentChar).AsInt();
                if (current == -1)
                {
                    return -1;
                }

                result = (result << 4) + current;
            }

            return result;
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static int AsInt(this byte charValue)
        {
            char currentChar = (char)charValue;
            if ((currentChar >= '0') && (currentChar <= '9'))
            {
                return currentChar - '0';
            }

            if ((currentChar >= 'a') && (currentChar <= 'f'))
            {
                return currentChar - 'a' + 10;
            }

            if ((currentChar >= 'A') && (currentChar <= 'F'))
            {
                return currentChar - 'A' + 10;
            }

            return -1;
        }

        [ExcludeFromCodeCoverage]
        [SuppressMessage("Microsoft.Design", "CA0000:ExcludeFromCodeCoverage", Justification = "Code is taken from an external source.")]
        private static void WriteCharBytes(this IList buffer, char currentChar, Encoding encoding)
        {
            if (currentChar > 255)
            {
                foreach (byte value in encoding.GetBytes(new[] { currentChar }))
                {
                    buffer.Add(value);
                }
            }
            else
            {
                buffer.Add((byte)currentChar);
            }
        }
    }
}
