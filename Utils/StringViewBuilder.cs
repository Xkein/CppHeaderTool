using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Utils
{
    /// <summary>
    /// String builder class that has support for StringView so that if a single instance of
    /// a StringView is appended, it is returned.
    /// </summary>
    public class StringViewBuilder
    {

        /// <summary>
        /// When only a string view has been appended, this references that StringView
        /// </summary>
        private StringView _stringView = new();

        /// <summary>
        /// Represents more complex data being appended
        /// </summary>
        private StringBuilder? _stringBuilder = null;

        /// <summary>
        /// Set to true when the builder has switched to a StringBuilder (NOTE: This can probably be removed)
        /// </summary>
        private bool _useStringBuilder = false;

        /// <summary>
        /// The length of the appended data
        /// </summary>
        public int Length
        {
            get
            {
                if (_useStringBuilder && _stringBuilder != null)
                {
                    return _stringBuilder.Length;
                }
                else
                {
                    return _stringView.Span.Length;
                }
            }
        }

        /// <summary>
        /// Return the appended data as a StringView
        /// </summary>
        /// <returns>Contents of the builder</returns>
        public StringView ToStringView()
        {
            return _useStringBuilder ? new StringView(_stringBuilder!.ToString()) : _stringView;
        }

        /// <summary>
        /// Return the appended data as a string
        /// </summary>
        /// <returns>Contents of the builder</returns>
        public override string ToString()
        {
            return _useStringBuilder ? _stringBuilder!.ToString() : _stringView.ToString();
        }

        /// <summary>
        /// Append a StringView
        /// </summary>
        /// <param name="text">Text to be appended</param>
        /// <returns>The string builder</returns>
        public StringViewBuilder Append(StringView text)
        {
            if (_useStringBuilder)
            {
                _stringBuilder!.Append(text.Span);
            }
            else if (_stringView.Span.Length > 0)
            {
                SwitchToStringBuilder();
                _stringBuilder!.Append(text.Span);
            }
            else
            {
                _stringView = text;
            }
            return this;
        }

        /// <summary>
        /// Append a character
        /// </summary>
        /// <param name="c">Character to be appended</param>
        /// <returns>The string builder</returns>
        public StringViewBuilder Append(char c)
        {
            SwitchToStringBuilder();
            _stringBuilder!.Append(c);
            return this;
        }

        /// <summary>
        /// If not already done, switch the builder to using a StringBuilder
        /// </summary>
        private void SwitchToStringBuilder()
        {
            if (!_useStringBuilder)
            {
                _stringBuilder ??= new StringBuilder();
                _useStringBuilder = true;
                _stringBuilder.Append(_stringView.Span);
            }
        }
    }
}
