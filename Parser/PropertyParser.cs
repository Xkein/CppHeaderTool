using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Types;
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


        public PropertyParser(CppField cppField)
        {
            this.cppField = cppField;
        }



        public override void Parse()
        {
            HtProperty htProperty = new HtProperty();
            htProperty.cppField = cppField;

            this.ParseMeta(cppField, metaData => PropertySpecifiers.ParseMeta(ref htProperty.meta, metaData));

            Session.typeTables.Add(htProperty);

        }
    }
}
