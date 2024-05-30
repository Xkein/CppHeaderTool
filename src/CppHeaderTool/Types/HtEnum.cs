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
        public string name => cppEnumItem.Name;
        public long value => cppEnumItem.Value;
    }
    public class HtEnum : HtType, IHasCppElement
    {
        public CppElement element => cppEnum;
        public CppEnum cppEnum;

        public List<HtEnumConstant> constants;

        public EnumMeta meta;

        public string name => cppEnum.Name;
        public string fullName => cppEnum.FullName;
        public bool isScoped => cppEnum.IsScoped;
        public bool isAnonymous => cppEnum.IsAnonymous;
        public int sizeOf => cppEnum.SizeOf;
        public CppVisibility visibility => cppEnum.Visibility;
        public bool isProtected => visibility == CppVisibility.Protected;
        public bool isPrivate => visibility == CppVisibility.Private;
        public string sourceFile
        {
            get
            {
                string file = cppEnum.SourceFile;
                return string.IsNullOrWhiteSpace(file) ? null : file;
            }
        }
    }
}
