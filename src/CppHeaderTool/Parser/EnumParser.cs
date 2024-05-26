using ClangSharp.Interop;
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
    internal class EnumParser : ParserBase
    {
        public CppEnum cppEnum { get; private set; }


        public EnumParser(CppEnum cppEnum)
        {
            this.cppEnum = cppEnum;
        }


        protected override string lockerName => TypeTables.GetUniqueName(cppEnum);
        protected override ValueTask ParseInternal()
        {
            if (Session.typeTables.TryGet(cppEnum, out _))
                return ValueTask.CompletedTask;

            Log.Verbose($"Parsing enum {cppEnum.FullName}");

            HtEnum htEnum = new HtEnum();
            htEnum.cppEnum = cppEnum;
            htEnum.constants = new List<HtEnumConstant>();

            this.ParseMeta(cppEnum, metaData => EnumSpecifiers.ParseMeta(ref htEnum.meta, metaData));

            ParseChildren(htEnum);

            Session.typeTables.Add(htEnum);

            return ValueTask.CompletedTask;
        }

        public static void ParseCursor(CXCursor cursor, CXCursor parent, CppEnum cppEnum)
        {

        }

        public static void ParseCursor(CXCursor cursor, CXCursor parent, CppEnumItem cppEnumItem)
        {

        }

        private void ParseChildren(HtEnum htEnum)
        {
            foreach (CppEnumItem cppEnumItem in cppEnum.Items)
            {
                HtEnumConstant htEnumConst = new HtEnumConstant();
                htEnumConst.cppEnumItem = cppEnumItem;

                this.ParseMeta(cppEnumItem, metaData => EnumConstantSpecifies.ParseMeta(ref htEnumConst.meta, metaData));

                htEnum.constants.Add(htEnumConst);
            }
        }
    }
}
