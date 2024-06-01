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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Parser
{
    internal class CppFieldUserObject
    {
        public HtProperty property;
        public bool isConstexpr;
    }
    internal class PropertyParser : ParserBase
    {
        public CppField cppField { get; private set; }

        protected override string lockerName => TypeTables.GetUniqueName(cppField);

        public PropertyParser(CppField cppField)
        {
            this.cppField = cppField;
        }

        protected override async ValueTask ParseInternal()
        {
            if (Session.typeTables.TryGet(cppField, out _))
                return;
            //Log.Information($"Parsing property {cppField.FullParentName}.{cppField}");

            HtProperty htProperty = new HtProperty();
            htProperty.cppField = cppField;
            htProperty.isConst = GetIsConst(cppField);

            var userData = cppField.GetUserData<CppFieldUserObject>();
            userData.property = htProperty;
            htProperty.isConstexpr = userData.isConstexpr;

            this.ParseMeta(cppField, metaData => PropertySpecifiers.ParseMeta(ref htProperty.meta, metaData));

            Session.typeTables.Add(htProperty);

            if (htProperty.isAnonymous)
            {
                CppType unwarpType = cppField.Type.UnwrapType();
                if (unwarpType is CppClass anonymousCppClass)
                {
                    var parser = new ClassParser(anonymousCppClass);
                    await parser.Parse();
                    if (Session.typeTables.TryGet(anonymousCppClass, out HtClass anonymousClass))
                    {
                        htProperty.anonymousClass = anonymousClass;
                    }
                }
            }

            return;
        }

        public static void ParseCursor(CXCursor cursor, CXCursor parent, CppField cppField)
        {
            var userData = new CppFieldUserObject();
            cppField.UserData = userData;

            if (cppField.InitExpression == null)
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
                else if (text == cppField.Name)
                {
                    break;
                }
                iter.Next();
            }
        }
        
        private static bool GetIsConst(CppField cppField)
        {
            string str = cppField.Type.GetDisplayName();
            if (str.Contains('&'))
                return false;
            int constPos = str.IndexOf("const");
            if (constPos == -1)
                return false;
            int ptrPos = str.IndexOf('*');
            if (ptrPos == -1)
                return constPos >= 0;
            return ptrPos < constPos;
        }
    }
}
