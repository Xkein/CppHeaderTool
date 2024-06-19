

using CppAst;
using CppHeaderTool.Types;
using Scriban.Runtime;

namespace CppHeaderTool.Templates
{
    class ScriptType : ScriptObject
    {
        public static CppType UnwrapType(CppType cppType)
        {
            return cppType.UnwrapType();
        }
        public static HtClass UnwrapClass(CppType cppType)
        {
            if (cppType.UnwrapType() is CppClass cppClass)
            {
                return Session.typeTables.TryGet(cppClass, out var klass) ? klass : null;
            }
            return null;
        }

        public static HtClass GetClass(string name)
        {
            if(Session.typeTables.TryGetClass(name, out var type))
            {
                return type;
            }
            return null;
        }

        public static HtEnum GetEnum(string name)
        {
            if (Session.typeTables.TryGetEnum(name, out var type))
            {
                return type;
            }
            return null;
        }

        public static HtFunction GetFunction(string name)
        {
            if (Session.typeTables.TryGetFunction(name, out var type))
            {
                return type;
            }
            return null;
        }

        public static HtProperty GetProperty(string name)
        {
            if (Session.typeTables.TryGetProperty(name, out var type))
            {
                return type;
            }
            return null;
        }
    }
}