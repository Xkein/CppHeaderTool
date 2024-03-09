using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Specifies
{
    public class ClassSpecifiers
    {

        public static void ParseMeta(ref ClassMeta meta, HtMetaData metaData)
        {
            CommonSpecifiers.ParseAccessibilityMeta(ref meta.Accessibility, metaData);
            CommonSpecifiers.ParseRelectionMeta(ref meta.Reflection, metaData);
            CommonSpecifiers.ParseSerializationMeta(ref meta.Serialization, metaData);

            meta.AllReflected = metaData.GetTag("AllReflected");
        }


    }
}
