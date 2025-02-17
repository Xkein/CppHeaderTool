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
    public class HtFunction : HtType, IHasCppElement
    {
        public CppElement element => cppFunction;
        public CppFunction cppFunction;
        public string uniqueName => TypeTables.GetUniqueName(cppFunction);

        public FunctionMeta meta;
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
    }
}
