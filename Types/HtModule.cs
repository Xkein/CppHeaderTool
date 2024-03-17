using CppAst;
using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtModule : HtType, IHasCppElement
    {
        public CppElement element => cppCompilation;
        public CppCompilation cppCompilation;
        public string moduleName;

        public List<HtClass> classes;
        public List<HtFunction> functions;
        public List<HtProperty> properties;
        public List<HtEnum> enums;
    }
}
