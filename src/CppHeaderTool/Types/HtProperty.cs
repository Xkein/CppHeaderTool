using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Parser;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
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
        public string uniqueName => TypeTables.GetUniqueName(cppField);
        public bool isStatic => cppField.StorageQualifier == CppStorageQualifier.Static;
        public bool isConst;
        public bool isConstexpr;
        public string name => cppField.Name;
        public bool isAnonymous => cppField.IsAnonymous;
        public HtClass anonymousClass;
        public bool isBitField => cppField.IsBitField;
        public int bitFieldWidth => cppField.BitFieldWidth;
        public long offset => cppField.Offset;
        public CppVisibility visibility => cppField.Visibility;
        public bool isProtected => visibility == CppVisibility.Protected;
        public bool isPrivate => visibility == CppVisibility.Private;
        public bool isArray => cppField.Type.TypeKind == CppTypeKind.Array;
        public bool isPointer => cppField.Type.TypeKind == CppTypeKind.Pointer;
        public bool isReference => cppField.Type.TypeKind == CppTypeKind.Reference;
        public bool isPrimitive => cppField.Type.TypeKind == CppTypeKind.Primitive;
        public CppType unwrapType => cppField.Type.UnwrapType();

        public HtClass unwrapClass
        {
            get
            {
                if (unwrapType is CppClass cppClass)
                {
                    return Session.typeTables.TryGet(cppClass, out var klass) ? klass : null;
                }
                return null;
            }
        }

        public PropertyMeta meta;
    }
}
