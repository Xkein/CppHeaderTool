using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public struct HtBaseClass : IHasCppElement
    {
        public CppElement element => cppBaseType;
        public CppBaseType cppBaseType;
        public HtClass klass;
        public string fullDisplayName => cppBaseType.Type.GetDisplayName();

        public HtBaseClass(CppBaseType cppBaseType)
        {
            this.cppBaseType = cppBaseType;
            if (cppBaseType.Type is CppClass cppClass)
            {
                Session.typeTables.TryGet(cppClass, out klass);
            }
        }
    }
    public class HtClass : HtType, IHasCppElement, IHasMeta
    {
        public CppElement element => cppClass;
        public CppClass cppClass;
        public string uniqueName => TypeTables.GetUniqueName(cppClass);

        public bool isAbstract => cppClass.IsAbstract;
        public bool isEmbeded => cppClass.IsEmbeded;
        public bool isDefinition => cppClass.IsDefinition;
        public bool isAnonymous => cppClass.IsAnonymous;
        public CppVisibility visibility => cppClass.Visibility;
        public bool isProtected => visibility == CppVisibility.Protected;
        public bool isPrivate => visibility == CppVisibility.Private;
        public bool isClass => cppClass.ClassKind == CppClassKind.Class;
        public bool isStruct => cppClass.ClassKind == CppClassKind.Struct;
        public bool isUnion => cppClass.ClassKind == CppClassKind.Union;
        public int alignOf => cppClass.AlignOf;
        public string name => cppClass.Name;
        public string fullName => cppClass.FullName;
        public string displayName => cppClass.GetDisplayName();
        public string sourceFile
        {
            get
            {
                string file = cppClass.SourceFile;
                return string.IsNullOrWhiteSpace(file) ? null : file;
            }
        }
        public bool isInterface;
        public IEnumerable<HtFunction> allFunctions => constructors.Concat(destructors).Concat(functions);
        public IEnumerable<HtFunction> allBaseFunctions
        {
            get
            {
                IEnumerable<HtFunction> result = Enumerable.Empty<HtFunction>();
                foreach (HtBaseClass baseClass in baseClasses)
                {
                    if (baseClass.klass == null)
                        continue;
                    result = result.Concat(baseClass.klass.allFunctions).Concat(baseClass.klass.allBaseFunctions);
                }
                return result;
            }
        }

        public Dictionary<string, List<HtFunction>> overloadFunctions;
        public List<HtFunction> overrideFunctions;

        public List<HtBaseClass> baseClasses;
        public List<HtFunction> functions;
        public List<HtFunction> destructors;
        public List<HtFunction> constructors;
        public List<HtProperty> properties;
        public List<HtEnum> enums;

        public List<HtClass> anonymousClasses;

        public ClassMeta meta;
        public ref RawMeta rawMeta => ref meta.Raw;

    }
}
