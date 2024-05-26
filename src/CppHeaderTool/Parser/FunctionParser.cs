using ClangSharp.Interop;
using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
using CppHeaderTool.Tokenizers;
using CppHeaderTool.Types;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Parser
{
    internal class FunctionParser : ParserBase
    {
        public CppFunction cppFunction { get; private set; }

        public FunctionParser(CppFunction cppFunction)
        {
            this.cppFunction = cppFunction;
        }


        protected override string lockerName => TypeTables.GetUniqueName(cppFunction);
        protected override ValueTask ParseInternal()
        {
            if (Session.typeTables.TryGet(cppFunction, out _))
                return ValueTask.CompletedTask;
            //Log.Information($"Parsing function {cppFunction.FullParentName}.{cppFunction}");

            HtFunction htFunction = new HtFunction();
            htFunction.cppFunction = cppFunction;

            ParserData userData = cppFunction.GetUserData<ParserData>();
            htFunction.isConstexpr = userData.isConstexpr;

            this.ParseMeta(cppFunction, metaData => FunctionSpecifiers.ParseMeta(ref htFunction.meta, metaData));

            Session.typeTables.Add(htFunction);

            return ValueTask.CompletedTask;
        }

        class ParserData
        {
            public bool isConstexpr;
        }
        public static void ParseCursor(CXCursor cursor, CXCursor parent, CppFunction cppFunction)
        {
            ParserData userData = new ParserData();
            cppFunction.UserData = userData;

            if (!cppFunction.Flags.HasFlag(CppFunctionFlags.Inline))
            {
                return;
            }

            Tokenizer tokenizer = new Tokenizer(cursor);
            TokenIterator iter = new TokenIterator(tokenizer);
            while (iter.CanPeek)
            {
                string text = iter.PeekText();
                if (text == "constexpr")
                {
                    userData.isConstexpr = true;
                }
                else if (text == "(")
                {
                    break;
                }
                iter.Next();
            }
        }
    }
}
