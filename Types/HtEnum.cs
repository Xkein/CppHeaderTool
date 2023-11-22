using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Types
{
    public enum HtEnumCppForm
    {
        /// <summary>
        /// enum Name {...}
        /// </summary>
        Regular,

        /// <summary>
        /// namespace Name { enum Type { ... } }
        /// </summary>
        Namespaced,

        /// <summary>
        /// enum class Name {...}
        /// </summary>
        EnumClass
    }

    /// <summary>
    /// Underlying type of the enumeration
    /// </summary>
    public enum HtEnumUnderlyingType
    {
        Unspecified,
        Uint8,
        Uint16,
        Uint32,
        Uint64,
        Int8,
        Int16,
        Int32,
        Int64,
        Int,
    }


    public struct HtEnumValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class HtEnum
    {
        public bool IsBitMask { get; set; } = false;

        public HtEnumCppForm CppForm { get; set; } = HtEnumCppForm.Regular;

        public HtEnumUnderlyingType Type { get; set; } = HtEnumUnderlyingType.Uint8;

        public List<HtEnumValue> EnumValues { get; }

        public HtEnum()
        {
            EnumValues = new List<HtEnumValue>();
        }
    }
}
