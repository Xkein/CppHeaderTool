using CppHeaderTool.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Utils
{
    static class HtStringBuilderExtensions
    {
        /// <summary>
        /// String of tabs used to generate code with proper indentation
        /// </summary>
        public static StringView TabsString = new(new string('\t', 128));

        /// <summary>
        /// String of spaces used to generate code with proper indentation
        /// </summary>
        public static StringView SpacesString = new(new string(' ', 128));

        /// <summary>
        /// Append tabs to the builder
        /// </summary>
        /// <param name="builder">Destination builder</param>
        /// <param name="tabs">Number of tabs to insert</param>
        /// <returns>Destination builder</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of tabs is out of range</exception>
        public static StringBuilder AppendTabs(this StringBuilder builder, int tabs)
        {
            if (tabs < 0 || tabs > TabsString.Length)
            {
                throw new ArgumentOutOfRangeException($"Number of tabs specified must be between 0 and {TabsString.Length - 1} inclusive");
            }
            else if (tabs > 0)
            {
                builder.Append(TabsString.Span[..tabs]);
            }
            return builder;
        }

        /// <summary>
        /// Append spaces to the builder
        /// </summary>
        /// <param name="builder">Destination builder</param>
        /// <param name="spaces">Number of spaces to insert</param>
        /// <returns>Destination builder</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of spaces is out of range</exception>
        public static StringBuilder AppendSpaces(this StringBuilder builder, int spaces)
        {
            if (spaces < 0 || spaces > SpacesString.Length)
            {
                throw new ArgumentOutOfRangeException($"Number of spaces specified must be between 0 and {SpacesString.Length - 1} inclusive");
            }
            else if (spaces > 0)
            {
                builder.Append(SpacesString.Span[..spaces]);
            }
            return builder;
        }

        /// <summary>
        /// Append a name declaration to the builder
        /// </summary>
        /// <param name="builder">Destination builder</param>
        /// <param name="namePrefix">Name prefix</param>
        /// <param name="name">Name</param>
        /// <param name="nameSuffix">Optional name suffix</param>
        /// <returns>Destination builder</returns>
        public static StringBuilder AppendNameDecl(this StringBuilder builder, string? namePrefix, string name, string? nameSuffix)
        {
            return builder.Append(namePrefix).Append(name).Append(nameSuffix);
        }

        /// <summary>
        /// Append a name definition to the builder
        /// </summary>
        /// <param name="builder">Destination builder</param>
        /// <param name="staticsName">Optional name of the statics block which will be output in the form of "StaticsName::"</param>
        /// <param name="namePrefix">Name prefix</param>
        /// <param name="name">Name</param>
        /// <param name="nameSuffix">Optional name suffix</param>
        /// <returns>Destination builder</returns>
        public static StringBuilder AppendNameDef(this StringBuilder builder, string? staticsName, string? namePrefix, string name, string? nameSuffix)
        {
            if (!String.IsNullOrEmpty(staticsName))
            {
                builder.Append(staticsName).Append("::");
            }
            return builder.AppendNameDecl(namePrefix, name, nameSuffix);
        }

        /// <summary>
        /// Append the given text as a UTF8 encoded string
        /// </summary>
        /// <param name="builder">Destination builder</param>
        /// <param name="useText">If false, don't encode the text but include a nullptr</param>
        /// <param name="text">Text to include or an empty string if null.</param>
        /// <returns>Destination builder</returns>
        public static StringBuilder AppendUTF8LiteralString(this StringBuilder builder, bool useText, string? text)
        {
            if (!useText)
            {
                builder.Append("nullptr");
            }
            else if (text == null)
            {
                builder.Append("");
            }
            else
            {
                builder.AppendUTF8LiteralString(text);
            }
            return builder;
        }

        /// <summary>
        /// Append the given text as a UTF8 encoded string
        /// </summary>
        /// <param name="builder">Destination builder</param>
        /// <param name="text">Text to be encoded</param>
        /// <returns>Destination builder</returns>
        public static StringBuilder AppendUTF8LiteralString(this StringBuilder builder, StringView text)
        {
            builder.Append('\"');

            ReadOnlySpan<char> span = text.Span;
            int length = span.Length;

            if (length > 0)
            {

                bool trailingHex = false;
                int index = 0;
                while (true)
                {
                    // Scan forward looking for anything that can just be blindly copied
                    int startIndex = index;
                    while (index < length)
                    {
                        char cskip = span[index];
                        if (cskip < 31 || cskip > 127 || cskip == '"' || cskip == '\\')
                        {
                            break;
                        }
                        ++index;
                    }

                    // If we found anything
                    if (startIndex < index)
                    {
                        // We close and open the literal here in order to ensure that successive hex characters aren't appended to the hex sequence, causing a different number
                        if (trailingHex && HtFCString.IsHexDigit(span[startIndex]))
                        {
                            builder.Append("\"\"");
                        }
                        builder.Append(span[startIndex..index]);
                    }

                    // We have either reached the end of the string, break
                    if (index == length)
                    {
                        break;
                    }

                    // This character requires special processing
                    char c = span[index++];
                    switch (c)
                    {
                        case '\r':
                            trailingHex = false;
                            break;
                        case '\n':
                            trailingHex = false;
                            builder.Append("\\n");
                            break;
                        case '\\':
                            trailingHex = false;
                            builder.Append("\\\\");
                            break;
                        case '\"':
                            trailingHex = false;
                            builder.Append("\\\"");
                            break;
                        default:
                            if (c < 31)
                            {
                                trailingHex = true;
                                //Builder.Append($"\\x{(uint)c:x2}");
                                builder.Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", (uint)c);
                            }
                            else
                            {
                                trailingHex = false;
                                if (Char.IsHighSurrogate(c))
                                {
                                    if (index == length)
                                    {
                                        builder.Append('?');
                                        break;
                                    }

                                    char clow = span[index];
                                    if (Char.IsLowSurrogate(clow))
                                    {
                                        ++index;
                                        builder.AppendEscapedUtf32((ulong)Char.ConvertToUtf32(c, clow));
                                        trailingHex = true;
                                    }
                                    else
                                    {
                                        builder.Append('?');
                                    }
                                }
                                else if (Char.IsLowSurrogate(c))
                                {
                                    builder.Append('?');
                                }
                                else
                                {
                                    builder.AppendEscapedUtf32(c);
                                    trailingHex = true;
                                }
                            }
                            break;
                    }
                }
            }

            builder.Append('\"');
            return builder;
        }

        /// <summary>
        /// Encode a single UTF32 value as UTF8 characters
        /// </summary>
        /// <param name="builder">Destination builder</param>
        /// <param name="c">Character to encode</param>
        /// <returns>Destination builder</returns>
        public static StringBuilder AppendEscapedUtf32(this StringBuilder builder, ulong c)
        {
            if (c < 0x80)
            {
                builder
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", c);
            }
            else if (c < 0x800)
            {
                builder
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0xC0 + (c >> 6))
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0x80 + (c & 0x3f));
            }
            else if (c < 0x10000)
            {
                builder
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0xE0 + (c >> 12))
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0x80 + ((c >> 6) & 0x3f))
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0x80 + (c & 0x3f));
            }
            else
            {
                builder
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0xF0 + (c >> 18))
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0x80 + ((c >> 12) & 0x3f))
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0x80 + ((c >> 6) & 0x3f))
                    .Append("\\x").AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", 0x80 + (c & 0x3f));
            }
            return builder;
        }
    }

    /// <summary>
    /// Provides a cache of StringBuilders
    /// </summary>
    public class StringBuilderCache
    {

        /// <summary>
        /// Cache of StringBuilders with large initial buffer sizes
        /// </summary>
        public static readonly StringBuilderCache Big = new(256, 256 * 1024);

        /// <summary>
        /// Cache of StringBuilders with small initial buffer sizes
        /// </summary>
        public static readonly StringBuilderCache Small = new(256, 1 * 1024);

        /// <summary>
        /// Capacity of the cache
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        /// Initial buffer size for new StringBuilders.  Resetting StringBuilders might result
        /// in the initial chunk size being smaller.
        /// </summary>
        private readonly int _initialBufferSize;

        /// <summary>
        /// Stack of cached StringBuilders
        /// </summary>
        private readonly Stack<StringBuilder> _stack;

        /// <summary>
        /// Create a new StringBuilder cache
        /// </summary>
        /// <param name="capacity">Maximum number of StringBuilders to cache</param>
        /// <param name="initialBufferSize">Initial buffer size for newly created StringBuilders</param>
        public StringBuilderCache(int capacity, int initialBufferSize)
        {
            _capacity = capacity;
            _initialBufferSize = initialBufferSize;
            _stack = new Stack<StringBuilder>(_capacity);
        }

        /// <summary>
        /// Borrow a StringBuilder from the cache.
        /// </summary>
        /// <returns></returns>
        public StringBuilder Borrow()
        {
            lock (_stack)
            {
                if (_stack.Count > 0)
                {
                    return _stack.Pop();
                }
            }

            return new StringBuilder(_initialBufferSize);
        }

        /// <summary>
        /// Return a StringBuilder to the cache
        /// </summary>
        /// <param name="builder">The builder being returned</param>
        public void Return(StringBuilder builder)
        {
            // Sadly, clearing the builder (sets length to 0) will reallocate chunks.
            builder.Clear();
            lock (_stack)
            {
                if (_stack.Count < _capacity)
                {
                    _stack.Push(builder);
                }
            }
        }
    }

    /// <summary>
    /// Structure to automate the borrowing and returning of a StringBuilder.
    /// Use some form of a "using" pattern.
    /// </summary>
    public struct BorrowStringBuilder : IDisposable
    {

        /// <summary>
        /// Owning cache
        /// </summary>
        private StringBuilderCache Cache { get; }

        /// <summary>
        /// Borrowed string builder
        /// </summary>
        public StringBuilder StringBuilder { get; }

        /// <summary>
        /// Borrow a string builder from the given cache
        /// </summary>
        /// <param name="cache">String builder cache</param>
        public BorrowStringBuilder(StringBuilderCache cache)
        {
            Cache = cache;
            StringBuilder = Cache.Borrow();
        }

        /// <summary>
        /// Return the string builder to the cache
        /// </summary>
        public void Dispose()
        {
            Cache.Return(StringBuilder);
        }
    }
}
