

using CppAst;
using CppHeaderTool.Meta;
using CppHeaderTool.Types;
using Scriban.Runtime;
using Serilog;

namespace CppHeaderTool.Templates
{
    class ScriptMeta : ScriptObject
    {
        public static void SetHasMeta(IHasMeta obj)
        {
            if (!obj.rawMeta.hasMeta)
            {
                obj.rawMeta.hasMeta = true;
                obj.rawMeta.metaData = new HtMetaData("unknown", new CppSourceSpan());
            }
        }
        public static void SetMetaTag(IHasMeta obj, string tag)
        {
            SetHasMeta(obj);
            obj.rawMeta.metaData.AddTag(tag);
        }
        public static void SetMetaValue(IHasMeta obj, string key, string val)
        {
            SetHasMeta(obj);
            obj.rawMeta.metaData.AddKeyValue(key, val);
        }
        public static void SetMetaStringList(IHasMeta obj, string key, string[] list)
        {
            SetHasMeta(obj);
            obj.rawMeta.metaData.AddStringList(key, list);
        }
    }
}