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
    public class HtProperty : HtType, IHasCppElement
    {
        public CppElement element => cppField;
        public CppField cppField;

        public PropertyMeta meta;
    }
}
