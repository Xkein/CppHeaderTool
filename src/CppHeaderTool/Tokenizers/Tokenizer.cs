using ClangSharp.Interop;
using CppAst;
using CppHeaderTool.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tokenizers
{
    /// <summary>
    /// Internal class to iterate on tokens
    /// </summary>
    internal class TokenIterator
    {
        private readonly Tokenizer _tokens;
        private int _index;

        public TokenIterator(Tokenizer tokens)
        {
            _tokens = tokens;
        }

        public bool Skip(string expectedText)
        {
            if (_index < _tokens.Count)
            {
                if (_tokens.GetString(_index) == expectedText)
                {
                    _index++;
                    return true;
                }
            }

            return false;
        }

        public CppToken PreviousToken()
        {
            if (_index > 0)
            {
                return _tokens[_index - 1];
            }

            return null;
        }

        public bool Skip(params string[] expectedTokens)
        {
            var startIndex = _index;
            foreach (var expectedToken in expectedTokens)
            {
                if (startIndex < _tokens.Count)
                {
                    if (_tokens.GetString(startIndex) == expectedToken)
                    {
                        startIndex++;
                        continue;
                    }
                }
                return false;
            }
            _index = startIndex;
            return true;
        }

        public bool Find(params string[] expectedTokens)
        {
            var startIndex = _index;
        restart:
            while (startIndex < _tokens.Count)
            {
                var firstIndex = startIndex;
                foreach (var expectedToken in expectedTokens)
                {
                    if (startIndex < _tokens.Count)
                    {
                        if (_tokens.GetString(startIndex) == expectedToken)
                        {
                            startIndex++;
                            continue;
                        }
                    }
                    startIndex = firstIndex + 1;
                    goto restart;
                }
                _index = firstIndex;
                return true;
            }
            return false;
        }

        public bool Next(out CppToken token)
        {
            token = null;
            if (_index < _tokens.Count)
            {
                token = _tokens[_index];
                _index++;
                return true;
            }
            return false;
        }

        public bool CanPeek => _index < _tokens.Count;

        public bool Next()
        {
            if (_index < _tokens.Count)
            {
                _index++;
                return true;
            }
            return false;
        }

        public CppToken Peek()
        {
            if (_index < _tokens.Count)
            {
                return _tokens[_index];
            }
            return null;
        }

        public string PeekText()
        {
            if (_index < _tokens.Count)
            {
                return _tokens.GetString(_index);
            }
            return null;
        }
    }

    /// <summary>
    /// Internal class to tokenize
    /// </summary>
    [DebuggerTypeProxy(typeof(TokenizerDebuggerType))]
    internal class Tokenizer
    {
        private readonly CXSourceRange _range;
        private CppToken[] _cppTokens;
        protected readonly CXTranslationUnit _tu;

        public Tokenizer(CXCursor cursor)
        {
            _tu = cursor.TranslationUnit;
            _range = GetRange(cursor);
        }

        public Tokenizer(CXTranslationUnit tu, CXSourceRange range)
        {
            _tu = tu;
            _range = range;
        }

        public virtual CXSourceRange GetRange(CXCursor cursor)
        {
            return cursor.Extent;
        }

        public int Count
        {
            get
            {
                var tokens = _tu.Tokenize(_range);
                int length = tokens.Length;
                _tu.DisposeTokens(tokens);
                return length;
            }
        }

        public CppToken this[int i]
        {
            get
            {
                // Only create a tokenizer if necessary
                if (_cppTokens == null)
                {
                    _cppTokens = new CppToken[Count];
                }

                ref var cppToken = ref _cppTokens[i];
                if (cppToken != null)
                {
                    return cppToken;
                }
                var tokens = _tu.Tokenize(_range);
                var token = tokens[i];

                CppTokenKind cppTokenKind = 0;
                switch (token.Kind)
                {
                    case CXTokenKind.CXToken_Punctuation:
                        cppTokenKind = CppTokenKind.Punctuation;
                        break;
                    case CXTokenKind.CXToken_Keyword:
                        cppTokenKind = CppTokenKind.Keyword;
                        break;
                    case CXTokenKind.CXToken_Identifier:
                        cppTokenKind = CppTokenKind.Identifier;
                        break;
                    case CXTokenKind.CXToken_Literal:
                        cppTokenKind = CppTokenKind.Literal;
                        break;
                    case CXTokenKind.CXToken_Comment:
                        cppTokenKind = CppTokenKind.Comment;
                        break;
                    default:
                        break;
                }

                var tokenStr = CXUtil.GetTokenSpelling(token, _tu);
                var tokenLocation = token.GetLocation(_tu);

                var tokenRange = token.GetExtent(_tu);
                cppToken = new CppToken(cppTokenKind, tokenStr)
                {
                    Span = new CppSourceSpan(CXUtil.GetSourceLocation(tokenRange.Start), CXUtil.GetSourceLocation(tokenRange.End))
                };
                _tu.DisposeTokens(tokens);
                return cppToken;
            }
        }

        public string GetString(int i)
        {
            var tokens = _tu.Tokenize(_range);
            var TokenSpelling = CXUtil.GetTokenSpelling(tokens[i], _tu);
            _tu.DisposeTokens(tokens);
            return TokenSpelling;
        }

        public string TokensToString()
        {
            int length = Count;
            if (length <= 0)
            {
                return null;
            }

            var tokens = new List<CppToken>(length);

            for (int i = 0; i < length; i++)
            {
                tokens.Add(this[i]);
            }

            return CppToken.TokensToString(tokens);
        }

        public string GetStringForLength(int length)
        {
            StringBuilder result = new StringBuilder(length);
            for (var cur = 0; cur < Count; ++cur)
            {
                result.Append(GetString(cur));
                if (result.Length >= length)
                    return result.ToString();
            }
            return result.ToString();
        }
    }

    class TokenizerDebuggerType
    {
        private readonly Tokenizer _tokenizer;

        public TokenizerDebuggerType(Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                var array = new object[_tokenizer.Count];
                for (int i = 0; i < _tokenizer.Count; i++)
                {
                    array[i] = _tokenizer[i];
                }
                return array;
            }
        }
    }
}
