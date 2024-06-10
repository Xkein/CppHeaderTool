using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Utils
{
    internal static class MetaUtils
    {
        public static bool TryParseMetaData(HtMetaData meta, string rawMeta)
        {
            int length = rawMeta.Length;

            bool isPair = false;
            bool isListEnd = false;
            StringBuilder builder = new StringBuilder();
            List<string> tmp = new List<string>(1);

            int idx = 0;
            while(idx < length)
            {
                bool isEndOrSeparate = idx + 1 == length;
                char c = rawMeta[idx];

                if (HtFCString.IsAlnum(c) || c == '_')
                {
                    builder.Append(c);
                }
                else if (c == ',')
                {
                    isEndOrSeparate = true;
                }
                else if (c == '=')
                {
                    isEndOrSeparate = true;
                    isPair = true;
                }
                else if (c == '\"')
                {
                    int strStartIdx = idx + 1;
                    if (strStartIdx >= length)
                    {
                        throw new Exception("unexpected end after '\"'");
                    }
                    idx = rawMeta.IndexOf('\"', strStartIdx);
                    if (idx < 0)
                    {
                        throw new Exception("error finding end of string");
                    }
                    if (idx > strStartIdx)
                    {
                        builder.Append(rawMeta.Substring(strStartIdx, idx - strStartIdx));
                    }
                    isEndOrSeparate = true;
                }
                else if (c == '/')
                {
                    if (idx + 1 >= length)
                    {
                        throw new Exception("unexpected end after '/'");
                    }
                    char next = rawMeta[++idx];
                    if (next == '/')
                    {
                        throw new Exception("unexpected '//'");
                    }
                    else if (next == '*' && idx + 1 < length)
                    {
                        idx = rawMeta.IndexOf("*/", ++idx);
                        if (idx < 0)
                        {
                            throw new Exception("error finding end of comment");
                        }
                    }
                    else
                    {
                        throw new Exception($"unexpected '{next}' after '/'");
                    }
                }
                else if (c == '[')
                {
                    isPair = false;
                }
                else if (c == ']')
                {
                    isEndOrSeparate = true;
                    isListEnd = true;
                }

                if (isEndOrSeparate)
                {
                    string word = builder.ToString();
                    tmp.Add(word);
                    builder.Clear();

                    if (isPair)
                    {
                        if (tmp.Count == 2)
                        {
                            meta.AddKeyValue(tmp[0], tmp[1]);
                            tmp.Clear();
                            isPair = false;
                        }
                        else if (tmp.Count > 2)
                        {
                            throw new Exception("not support embed key value pairs!");
                        }
                    }
                    else
                    {
                        if (tmp.Count <= 1)
                        {
                            meta.AddTag(tmp[0]);
                            tmp.Clear();
                        }
                        else if (isListEnd)
                        {
                            meta.AddStringList(tmp[0], tmp[1..].ToArray());
                            tmp.Clear();
                            isListEnd = false;
                        }
                    }
                }

                idx++;
            }

            return true;
        }
    }
}
