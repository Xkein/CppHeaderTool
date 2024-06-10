

using CppHeaderTool.Types;
using Scriban.Runtime;

namespace CppHeaderTool.Templates
{
    class ScriptType : ScriptObject
    {
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