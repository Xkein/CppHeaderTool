using CppAst;
using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtClass : HtType, IHasCppElement
    {
        public CppElement element => cppClass;
        public CppClass cppClass;

        public List<HtFunction> functions;
        public List<HtProperty> properties;

        public ClassMeta meta;


    }
}
