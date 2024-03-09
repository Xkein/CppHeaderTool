using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Specifies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtEnumConstant : HtType, IHasCppElement
    {
        public CppElement element => cppEnumItem;
        public CppEnumItem cppEnumItem;

        public EnumConstantMeta meta;
    }
    public class HtEnum : HtType, IHasCppElement
    {
        public CppElement element => cppEnum;
        public CppEnum cppEnum;

        public List<HtEnumConstant> constants;

        public EnumMeta meta;
    }
}
