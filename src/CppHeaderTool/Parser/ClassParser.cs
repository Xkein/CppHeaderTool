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
    class CppClassUserObject
    {
        public HtClass klass;
    }
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
            htClass.anonymousClasses = new List<HtClass>();
            htClass.anonymousInlineProperties = new List<HtProperty>();
            htClass.isInterface = htClass.isAbstract && cppClass.Name.StartsWith('I');

            CppClassUserObject userObject = cppClass.GetUserData<CppClassUserObject>();
            userObject.klass = htClass;

            this.ParseMeta(cppClass, metaData => ClassSpecifiers.ParseMeta(ref htClass.meta, metaData));

            await ParseChildren(htClass);

            Session.typeTables.Add(htClass);
        }

        public static void ParseCursor(UserCustomParseContext context, CppClass cppClass)
        {
            cppClass.UserData = new CppClassUserObject();

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

            Dictionary<string, List<HtFunction>> overloadFunctions = new();
            Dictionary<string, HtFunction> funcDict = new();
            foreach (HtFunction htFunction in htClass.allFunctions)
            {
                if (funcDict.TryGetValue(htFunction.name, out HtFunction otherFunc))
                {
                    if (overloadFunctions.TryGetValue(htFunction.name, out List<HtFunction> funcList))
                    {
                        htFunction.isOverload = true;
                        funcList.Add(htFunction);
                    }
                    else
                    {
                        htFunction.isOverload = true;
                        otherFunc.isOverload = true;
                        overloadFunctions[htFunction.name] = new List<HtFunction>() { htFunction, otherFunc };
                    }
                }
                else
                {
                    funcDict.Add(htFunction.name, htFunction);
                }
            }
            htClass.overloadFunctions = overloadFunctions;

            List<HtFunction> overrideFunctions = new();
            foreach (HtFunction htFunction in htClass.allBaseFunctions)
            {
                if (funcDict.TryGetValue(htFunction.name, out HtFunction myFunction))
                {
                    myFunction.isOverride = true;
                    overrideFunctions.Add(myFunction);
                }
            }
            htClass.overrideFunctions = overrideFunctions;

            foreach (CppField cppField in cppClass.Fields)
            {
                if (typeTables.TryGet(cppField, out HtProperty htProperty))
                {
                    htClass.properties.Add(htProperty);
                    if (htProperty.anonymousClass != null)
                    {
                        htClass.anonymousClasses.Add(htProperty.anonymousClass);
                    }
                }
            }

            foreach (CppEnum cppEnum in cppClass.Enums)
            {
                if (typeTables.TryGet(cppEnum, out HtEnum htEnum))
                {
                    htClass.enums.Add(htEnum);
                }
            }
            
            SetAnonymousInlineProperties(htClass, htClass.anonymousInlineProperties);
        }

        private static void SetAnonymousInlineProperties(HtClass htClass, List<HtProperty> list)
        {
            foreach (HtProperty htProperty in htClass.properties)
            {
                if (htClass.isAnonymous && string.IsNullOrEmpty(htClass.name))
                {
                    list.Add(htProperty);
                }
                if (htProperty.anonymousClass == null || !string.IsNullOrEmpty(htProperty.name))
                    continue;
                foreach (HtProperty inlineProperty in htProperty.anonymousClass.properties)
                {
                    if (inlineProperty.anonymousClass == null || !string.IsNullOrEmpty(inlineProperty.name))
                    {
                        list.Add(inlineProperty);
                    }
                    else
                    {
                        SetAnonymousInlineProperties(inlineProperty.anonymousClass, list);
                    }
                }
            }
        }
    }
}
