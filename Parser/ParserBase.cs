using CppAst;
using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Parser
{
    internal abstract class ParserBase
    {
        public abstract void Parse();
    }


    internal static class ParserExtensions
    {
        public static void ParseMeta(this ParserBase parser, CppElement element, Action<HtMetaData> parseFunc)
        {
            CppSourceLocation sourceLocation = element.Span.Start;
            if (!Session.metaTables.Tables.TryGetValue(sourceLocation.File, out var metaTable))
                return;

            for (int offset = -1; offset <= 0; offset++)
            {
                if (metaTable.TryGetValue(sourceLocation.Line + offset, out HtMetaData lineMeta))
                {
                    parseFunc(lineMeta);
                    return;
                }
            }
        }
    }
}
