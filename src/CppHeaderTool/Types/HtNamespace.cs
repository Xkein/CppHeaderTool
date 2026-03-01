using CppAst;
using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtNamespace : HtType, IHasCppElement
    {
        public CppElement element => cppNamespace;
        public CppNamespace cppNamespace;

        public string uniqueName => fullName;
        public string fullName
        {
            get
            {
                string fullparent = cppNamespace.FullParentName;
                return string.IsNullOrEmpty(fullparent) ? cppNamespace.Name :$"{fullparent}::{cppNamespace.Name}";
            }
        }
        
        private string _identifier;
        public string identifier
        {
            get
            {   if (_identifier == null)
                {
                    _identifier = fullName.Replace('<', '_').Replace('>', '_').Replace(':', '_').Replace('*', '_').Replace(" ", "");
                }
                return _identifier;
            }
        }

        public List<HtClass> classes;
        public List<HtFunction> functions;
        public List<HtProperty> properties;
        public List<HtEnum> enums;
    }
}
