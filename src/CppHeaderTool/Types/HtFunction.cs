using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public class HtFunction : HtType, IHasCppElement, IHasMeta
    {
        public CppElement element => cppFunction;
        public CppFunction cppFunction;
        public string uniqueName => TypeTables.GetUniqueName(cppFunction);

        public FunctionMeta meta;
        public ref RawMeta rawMeta => ref meta.Raw;
        public bool isOverload;
        public bool isOverride;
        public bool isDeleted => cppFunction.Flags.HasFlag(CppFunctionFlags.Deleted);
        public bool isConst => cppFunction.IsConst;
        public bool isStatic => cppFunction.IsStatic;
        public bool isVirtual  => cppFunction.IsVirtual;
        public bool isPureVirtual  => cppFunction.IsPureVirtual;
        public bool isCxxClassMethod  => cppFunction.IsCxxClassMethod;
        public bool isDestructor => cppFunction.IsDestructor;
        public bool isConstructor => cppFunction.IsConstructor;
        public bool isFunctionTemplate => cppFunction.IsFunctionTemplate;
        public CppVisibility visibility => cppFunction.Visibility;
        public bool isProtected => visibility == CppVisibility.Protected;
        public bool isPrivate => visibility == CppVisibility.Private;
        public bool isConstexpr;
        public string name  => cppFunction.Name;
        
        public string fullName
        {
            get
            {
                string fullparent = cppFunction.FullParentName;
                return string.IsNullOrEmpty(fullparent) ? cppFunction.Name :$"{fullparent}::{cppFunction.Name}";
            }
        }
        
        private string _identifier;
        public string identifier
        {
            get
            {
                if (_identifier == null)
                {
                    _identifier = fullName.Replace('<', '_')
                        .Replace('>', '_')
                        .Replace(':', '_')
                        .Replace('*', '_')
                        .Replace(" ", "")
                        .Replace("[", "_").Replace("]", "_");
                }
                return _identifier;
            }
        }
    }
}
