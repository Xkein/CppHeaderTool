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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            htProperty.isConst = GetIsConst(cppField);

            ParserData userData = cppField.GetUserData<ParserData>();
            htProperty.isConstexpr = userData.isConstexpr;

            this.ParseMeta(cppField, metaData => PropertySpecifiers.ParseMeta(ref htProperty.meta, metaData));

            Session.typeTables.Add(htProperty);

            return ValueTask.CompletedTask;
        }

        class ParserData
        {
            public bool isConstexpr;
        }
        public static void ParseCursor(CXCursor cursor, CXCursor parent, CppField cppField)
        {
            ParserData userData = new ParserData();
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
        
        private bool GetIsConst(CppField cppField)
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
