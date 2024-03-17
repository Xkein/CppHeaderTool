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
        public abstract ValueTask Parse();
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

        public static async ValueTask ParseList(this ParserBase thisParser, CppContainerList<CppFunction> list)
        {
            foreach (CppFunction cppFunction in list)
            {
                var parser = new FunctionParser(cppFunction);
                await parser.Parse();
            }
        }
        public static async ValueTask ParseList(this ParserBase thisParser, CppContainerList<CppField> list)
        {
            foreach (CppField cppField in list)
            {
                var parser = new PropertyParser(cppField);
                await parser.Parse();
            }
        }
        public static async ValueTask ParseList(this ParserBase thisParser, CppContainerList<CppEnum> list)
        {
            foreach (CppEnum cppEnum in list)
            {
                var parser = new EnumParser(cppEnum);
                await parser.Parse();
            }
        }
        public static async ValueTask ParseList(this ParserBase thisParser, CppContainerList<CppClass> list)
        {
            await Parallel.ForEachAsync(list, async (CppClass cppClass, CancellationToken token) =>
            {
                var parser = new ClassParser(cppClass);
                await parser.Parse();
            });
        }
    }
}
