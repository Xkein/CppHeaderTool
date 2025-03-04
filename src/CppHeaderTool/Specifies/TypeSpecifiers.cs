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
            CommonSpecifiers.ParseRawMeta(ref meta.Raw, metaData);

            // meta.AllVisible = metaData.GetTag("AllVisible");
        }
    }
    public class EnumConstantSpecifies
    {
        public static void ParseMeta(ref EnumConstantMeta meta, HtMetaData metaData)
        {
            CommonSpecifiers.ParseRawMeta(ref meta.Raw, metaData);

            // meta.DisplayName = metaData.GetString("DisplayName");
        }
    }
    public class EnumSpecifiers
    {
        public static void ParseMeta(ref EnumMeta meta, HtMetaData metaData)
        {
            CommonSpecifiers.ParseAccessibilityMeta(ref meta.Accessibility, metaData);
            CommonSpecifiers.ParseRelectionMeta(ref meta.Reflection, metaData);
            CommonSpecifiers.ParseSerializationMeta(ref meta.Serialization, metaData);
            CommonSpecifiers.ParseRawMeta(ref meta.Raw, metaData);

        }

    }
    public class FunctionSpecifiers
    {

        public static void ParseMeta(ref FunctionMeta meta, HtMetaData metaData)
        {
            CommonSpecifiers.ParseAccessibilityMeta(ref meta.Accessibility, metaData);
            CommonSpecifiers.ParseRelectionMeta(ref meta.Reflection, metaData);
            CommonSpecifiers.ParseSerializationMeta(ref meta.Serialization, metaData);
            CommonSpecifiers.ParseRawMeta(ref meta.Raw, metaData);

        }

    }
    public class PropertySpecifiers
    {
        public static void ParseMeta(ref PropertyMeta meta, HtMetaData metaData)
        {
            CommonSpecifiers.ParseAccessibilityMeta(ref meta.Accessibility, metaData);
            CommonSpecifiers.ParseRelectionMeta(ref meta.Reflection, metaData);
            CommonSpecifiers.ParseSerializationMeta(ref meta.Serialization, metaData);
            CommonSpecifiers.ParseRawMeta(ref meta.Raw, metaData);

        }

    }
}
