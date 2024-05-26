using ClangSharp.Interop;
using CppAst;
using CppHeaderTool.Meta;
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
    internal class ClassParser : ParserBase
    {
        public CppClass cppClass { get; private set; }

        public ClassParser(CppClass cppClass)
        {
            this.cppClass = cppClass;
        }


        protected override string lockerName => TypeTables.GetUniqueName(cppClass);
        protected override async ValueTask ParseInternal()
        {
            if (Session.typeTables.TryGet(cppClass, out _))
                return;

            Log.Verbose($"Parsing class {cppClass.FullName}");

            HtClass htClass = new HtClass();
            htClass.cppClass = cppClass;
            htClass.baseClasses = new List<HtBaseClass>();
            htClass.functions = new List<HtFunction>();
            htClass.destructors = new List<HtFunction>();
            htClass.constructors = new List<HtFunction>();
            htClass.properties = new List<HtProperty>();
            htClass.enums = new List<HtEnum>();
            htClass.isInterface = htClass.isAbstract && cppClass.Name.StartsWith('I');

            this.ParseMeta(cppClass, metaData => ClassSpecifiers.ParseMeta(ref htClass.meta, metaData));

            await ParseChildren(htClass);

            Session.typeTables.Add(htClass);
        }

        public static void ParseCursor(CXCursor cursor, CXCursor parent, CppClass cppClass)
        {

        }

        private async Task ParseChildren(HtClass htClass)
        {
            // this.ParseList(cppClass.Classes);
            await Task.WhenAll(
                this.ParseList(cppClass.BaseTypes.Select(b => b.Type as CppClass).Where(c => c != null)).AsTask(),
                this.ParseList(cppClass.Constructors).AsTask(),
                this.ParseList(cppClass.Functions).AsTask(),
                this.ParseList(cppClass.Fields).AsTask(),
                this.ParseList(cppClass.Enums).AsTask()
            );

            var typeTables = Session.typeTables;

            foreach (CppBaseType cppBaseType in cppClass.BaseTypes)
            {
                htClass.baseClasses.Add(new HtBaseClass(cppBaseType));
            }
            foreach (CppFunction cppFunction in cppClass.Constructors)
            {
                if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                {
                    htClass.constructors.Add(htFunction);
                }
            }
            foreach (CppFunction cppFunction in cppClass.Destructors)
            {
                if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                {
                    htClass.destructors.Add(htFunction);
                }
            }
            foreach (CppFunction cppFunction in cppClass.Functions)
            {
                if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                {
                    htClass.functions.Add(htFunction);
                }
            }

            Dictionary<string, HtFunction> funcDict = new();
            foreach (HtFunction htFunction in htClass.allFunctions)
            {
                if (funcDict.TryGetValue(htFunction.cppFunction.Name, out HtFunction otherFunc))
                {
                    htFunction.isOverload = true;
                    otherFunc.isOverload = true;
                }
                else
                {
                    funcDict.Add(htFunction.cppFunction.Name, htFunction);
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
