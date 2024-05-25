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
        private Dictionary<string, object> _lockers = new();

        protected abstract string lockerName { get; }

        protected abstract ValueTask ParseInternal();
        public ValueTask Parse()
        {
            if (lockerName == null)
                return ParseInternal();

            if (!_lockers.TryGetValue(lockerName, out object locker))
            {
                lock (_lockers)
                {
                    _lockers.Add(lockerName, new object());
                    locker = _lockers[lockerName];
                }
            }
            lock (locker)
            {
                return ParseInternal();
            }
        }
    }


    internal static class ParserExtensions
    {
        public static void ParseMeta(this ParserBase parser, CppElement element, Action<HtMetaData> parseFunc)
        {
            if (element.SourceFile == null)
                return;

            CppSourceLocation sourceLocation = element.Span.Start;
            if (!Session.metaTables.TryGetValue(Path.GetFullPath(sourceLocation.File), out var metaTable))
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

        public static async ValueTask ParseList(this ParserBase thisParser, IEnumerable<CppFunction> list)
        {
            foreach (CppFunction cppFunction in list)
            {
                var parser = new FunctionParser(cppFunction);
                await parser.Parse();
            }
        }
        public static async ValueTask ParseList(this ParserBase thisParser, IEnumerable<CppField> list)
        {
            foreach (CppField cppField in list)
            {
                var parser = new PropertyParser(cppField);
                await parser.Parse();
            }
        }
        public static async ValueTask ParseList(this ParserBase thisParser, IEnumerable<CppEnum> list)
        {
            foreach (CppEnum cppEnum in list)
            {
                var parser = new EnumParser(cppEnum);
                await parser.Parse();
            }
        }
        public static async ValueTask ParseList(this ParserBase thisParser, IEnumerable<CppClass> list)
        {
            await Parallel.ForEachAsync(list, async (CppClass cppClass, CancellationToken token) =>
            {
                var parser = new ClassParser(cppClass);
                await parser.Parse();
            });
        }
    }
}
