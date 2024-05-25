using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
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
            //htFunction.isConstexpr = cppFunction.TokenAttributes;

            this.ParseMeta(cppFunction, metaData => FunctionSpecifiers.ParseMeta(ref htFunction.meta, metaData));

            Session.typeTables.Add(htFunction);

            return ValueTask.CompletedTask;
        }
    }
}
