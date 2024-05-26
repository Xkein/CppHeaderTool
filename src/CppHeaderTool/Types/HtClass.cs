using CppAst;
using CppHeaderTool.Meta;
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
        public string fullDisplayName
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(cppBaseType.Type.GetDisplayName());
                var cls = cppBaseType.Type as CppClass;
                if (cls != null && cls.TemplateKind != CppTemplateKind.NormalClass)
                {
                    builder.Append("<");

                    if (cls.TemplateKind == CppTemplateKind.TemplateSpecializedClass)
                    {
                        for (var i = 0; i < cls.TemplateSpecializedArguments.Count; i++)
                        {
                            if (i > 0) builder.Append(", ");
                            builder.Append(cls.TemplateSpecializedArguments[i].ToString());
                        }
                    }
                    else if (cls.TemplateKind == CppTemplateKind.TemplateClass)
                    {
                        for (var i = 0; i < cls.TemplateParameters.Count; i++)
                        {
                            if (i > 0) builder.Append(", ");
                            builder.Append(cls.TemplateParameters[i].ToString());
                        }
                    }

                    builder.Append(">");
                }
                return builder.ToString();
            }
        }

        public HtBaseClass(CppBaseType cppBaseType)
        {
            this.cppBaseType = cppBaseType;
            if (cppBaseType.Type is CppClass cppClass)
            {
                Session.typeTables.TryGet(cppClass, out klass);
            }
        }
    }
    public class HtClass : HtType, IHasCppElement
    {
        public CppElement element => cppClass;
        public CppClass cppClass;

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
        public string displayName => cppClass.GetDisplayName();

        public bool isInterface;
        public IEnumerable<HtFunction> allFunctions => constructors.Concat(destructors).Concat(functions);

        public List<HtBaseClass> baseClasses;
        public List<HtFunction> functions;
        public List<HtFunction> destructors;
        public List<HtFunction> constructors;
        public List<HtProperty> properties;
        public List<HtEnum> enums;

        public ClassMeta meta;


    }
}
