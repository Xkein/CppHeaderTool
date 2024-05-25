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
    internal class PropertyParser : ParserBase
    {
        public CppField cppField { get; private set; }

        protected override string lockerName => TypeTables.GetUniqueName(cppField);

        public PropertyParser(CppField cppField)
        {
            this.cppField = cppField;
        }

        protected override ValueTask ParseInternal()
        {
            if (Session.typeTables.TryGet(cppField, out _))
                return ValueTask.CompletedTask;
            //Log.Information($"Parsing property {cppField.FullParentName}.{cppField}");

            HtProperty htProperty = new HtProperty();
            htProperty.cppField = cppField;

            this.ParseMeta(cppField, metaData => PropertySpecifiers.ParseMeta(ref htProperty.meta, metaData));

            Session.typeTables.Add(htProperty);

            return ValueTask.CompletedTask;
        }
    }
}
