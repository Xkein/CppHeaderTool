using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Specifies;
using CppHeaderTool.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Parser
{
    internal class ClassParser : ParserBase
    {
        public CppClass cppClass { get; private set; }

        public ClassParser(CppClass cppClass)
        {
            this.cppClass = cppClass;
        }



        public override void Parse()
        {
            HtClass htClass = new HtClass();
            htClass.cppClass = cppClass;

            this.ParseMeta(cppClass, metaData => ClassSpecifiers.ParseMeta(ref htClass.meta, metaData));

            ParseChildren(htClass);

            Session.typeTables.Add(htClass);
        }

        private void ParseChildren(HtClass htClass)
        {
            foreach (CppFunction cppFunction in cppClass.Functions)
            {
                var parser = new FunctionParser(cppFunction);
                parser.Parse();
            }

            foreach (CppField cppField in cppClass.Fields)
            {
                var parser = new PropertyParser(cppField);
                parser.Parse();
            }

            var typeTables = Session.typeTables;

            foreach (CppFunction cppFunction in cppClass.Functions)
            {
                if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                {
                    htClass.functions.Add(htFunction);
                }
            }

            foreach (CppField cppField in cppClass.Fields)
            {
                if (typeTables.TryGet(cppField, out HtProperty htProperty))
                {
                    htClass.properties.Add(htProperty);
                }
            }

        }
    }
}
