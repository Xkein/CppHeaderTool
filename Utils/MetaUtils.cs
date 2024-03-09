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
        public static HtMetaData ParseMetaData(string rawMeta)
        {
            HtMetaData meta = new HtMetaData();

            int length = rawMeta.Length;

            bool isPair = false;
            StringBuilder builder = new StringBuilder();
            List<string> tmp = new List<string>(1);

            int idx = 0;
            while(idx < length)
            {
                char c = rawMeta[idx];

                if (HtFCString.IsAlnum(c) || c == '_')
                {
                    builder.Append(c);
                }
                else if (c == ',' || idx + 1 == length)
                {
                    string word = builder.ToString();
                    tmp.Add(word);
                    builder.Clear();

                    if (isPair)
                    {
                        meta.AddKeyValue(tmp[0], tmp[1]);
                        isPair = false;
                        tmp.Clear();
                    }
                    else
                    {
                        meta.AddTag(tmp[0]);
                        tmp.Clear();
                    }
                }
                else if (c == '=')
                {
                    string word = builder.ToString();
                    tmp.Add(word);
                    builder.Clear();

                    isPair = true;
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

                idx++;
            }

            return meta;
        }
    }
}
