using CppAst;
using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtModule : HtType
    {
        public string moduleName;

        public List<HtClass> classes;
        public List<HtFunction> functions;
        public List<HtProperty> properties;
        public List<HtEnum> enums;
        public List<HtNamespace> namespaces;
    }
}
