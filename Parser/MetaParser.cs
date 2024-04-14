using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
using CppHeaderTool.Utils;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Parser
{
    internal class MetaParser : ParserBase
    {
        public string filePath { get; private set; }

        private static List<string> metaKeywords = new (){
            Keywords.Class,
            Keywords.Enum,
            Keywords.Function,
            Keywords.Property,
        };

        internal FileMetaTables fileMetaTables
        {
            get
            {
                if (_FileMetaTables != null)
                    return _FileMetaTables;
                return _FileMetaTables = Session.metaTables.GetOrAdd(Path.GetFullPath(filePath), (key) => new FileMetaTables());
            }
        }

        private FileMetaTables _FileMetaTables;

        public MetaParser(string filePath)
        {
            this.filePath = filePath;
        }

        public override async ValueTask Parse()
        {
            int curLine = 1;
            IEnumerable<string> lines = await File.ReadAllLinesAsync(filePath);
            foreach (string line in lines)
            {
                ParseLine(line, curLine++);
            }
        }

        private void ParseLine(string line, int curLine)
        {
            if (line.Length == 0)
                return;

            int keywordIdx = -1;
            int keywordLen = 0;
            string keyword = null;
            foreach (string cur in metaKeywords)
            {
                keyword = cur;
                int idx = line.IndexOf(keyword);
                if (idx >= 0)
                {
                    keywordIdx = idx;
                    keywordLen = keyword.Length;
                    break;
                }
            }

            if (keywordIdx < 0 || keywordLen == 0)
                return;

            int bracketLeftIdx = line.IndexOf('(', keywordIdx + keywordLen);
            int bracketRightIdx = line.IndexOf(')', keywordIdx + keywordLen);

            if (bracketLeftIdx < 0 || bracketRightIdx < 0 || bracketLeftIdx > bracketRightIdx)
                return;

            try
            {
                StringView rawMeta = new StringView(line, bracketLeftIdx + 1, bracketRightIdx - bracketLeftIdx - 1);
                HtMetaData meta = new HtMetaData(keyword, new CppSourceSpan(
                    new CppSourceLocation(filePath, 0, curLine, keywordIdx + 1),
                    new CppSourceLocation(filePath, 0, curLine, bracketRightIdx + 1)));
                MetaUtils.TryParseMetaData(meta, rawMeta.ToString());

                if (!fileMetaTables.TryAdd(curLine, meta))
                {
                    Log.Error($"{meta.sourceSpan} could not add to meta table");
                }
                else
                {
                    Log.Verbose($"{meta.sourceSpan}: {keyword}({rawMeta})");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"{filePath}(line {curLine}): could not parse meta from \"{line}\"");
            }
        }
    }
}
