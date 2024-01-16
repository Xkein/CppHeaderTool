using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Specifies
{
    public class FunctionSpecifiers
    {

        public static void ParseMeta(ref FunctionMeta meta, HtMetaData metaData)
        {
            CommonSpecifiers.ParseAccessibilityMeta(ref meta.Accessibility, metaData);
            CommonSpecifiers.ParseRelectionMeta(ref meta.Reflection, metaData);
            CommonSpecifiers.ParseSerializationMeta(ref meta.Serialization, metaData);

        }

    }
}
