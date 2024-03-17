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



        public override async ValueTask Parse()
        {
            HtClass htClass = new HtClass();
            htClass.cppClass = cppClass;

            this.ParseMeta(cppClass, metaData => ClassSpecifiers.ParseMeta(ref htClass.meta, metaData));

            await ParseChildren(htClass);

            Session.typeTables.Add(htClass);
        }

        private async Task ParseChildren(HtClass htClass)
        {
            // this.ParseList(cppClass.Classes);
            await Task.WhenAll(
                this.ParseList(cppClass.Constructors).AsTask(),
                this.ParseList(cppClass.Functions).AsTask(),
                this.ParseList(cppClass.Fields).AsTask(),
                this.ParseList(cppClass.Enums).AsTask()
            );

            var typeTables = Session.typeTables;

            foreach (CppFunction cppFunction in cppClass.Constructors)
            {
                if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                {
                    htClass.functions.Add(htFunction);
                }
            }
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

            foreach (CppEnum cppEnum in cppClass.Enums)
            {
                if (typeTables.TryGet(cppEnum, out HtEnum htEnum))
                {
                    htClass.enums.Add(htEnum);
                }
            }
        }
    }
}
