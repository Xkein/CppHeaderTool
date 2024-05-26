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
        public bool isStatic => cppField.StorageQualifier == CppStorageQualifier.Static;
        public bool isConst;
        public bool isConstexpr;
        public string name => cppField.Name;
        public bool isAnonymous => cppField.IsAnonymous;
        public bool isBitField => cppField.IsBitField;
        public int bitFieldWidth => cppField.BitFieldWidth;
        public long offset => cppField.Offset;
        public CppVisibility visibility => cppField.Visibility;
        public bool isProtected => visibility == CppVisibility.Protected;
        public bool isPrivate => visibility == CppVisibility.Private;

        public PropertyMeta meta;
    }
}
