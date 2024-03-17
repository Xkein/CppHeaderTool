﻿using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Types;
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



        public override ValueTask Parse()
        {
            Console.WriteLine($"Parsing function {cppFunction.FullParentName}.{cppFunction.Name}");

            HtFunction htFunction = new HtFunction();
            htFunction.cppFunction = cppFunction;

            this.ParseMeta(cppFunction, metaData => FunctionSpecifiers.ParseMeta(ref htFunction.meta, metaData));

            Session.typeTables.Add(htFunction);

            return ValueTask.CompletedTask;
        }
    }
}
