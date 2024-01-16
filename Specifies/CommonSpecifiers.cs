using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Specifies
{
    internal class CommonSpecifiers
    {
        public static void ParseAccessibilityMeta(ref AccessibilityMeta meta, HtMetaData metaData)
        {
            //meta.visible = metaData.GetTag("Visible");

        }

        public static void ParseRelectionMeta(ref ReflectionMeta meta, HtMetaData metaData)
        {
            meta.Reflected = metaData.GetOptionalBool("Reflected", true);
        }

        public static void ParseSerializationMeta(ref SerializationMeta meta, HtMetaData metaData)
        {
            meta.Serializable = metaData.GetOptionalBool("Serializable", true);

        }
    }

}
