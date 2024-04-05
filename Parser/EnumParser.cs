using CppAst;
using CppHeaderTool.Specifies;
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



        public override ValueTask Parse()
        {
            Log.Information($"Parsing enum {cppEnum.FullName}");

            HtEnum htEnum = new HtEnum();
            htEnum.cppEnum = cppEnum;

            this.ParseMeta(cppEnum, metaData => EnumSpecifiers.ParseMeta(ref htEnum.meta, metaData));

            ParseChildren(htEnum);

            Session.typeTables.Add(htEnum);

            return ValueTask.CompletedTask;
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
