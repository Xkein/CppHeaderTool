using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Meta
{
    public struct AccessibilityMeta
    {
        //public bool visible;
    }

    public struct ReflectionMeta
    {
        public bool Reflected;

    }

    public struct SerializationMeta
    {
        public bool Serializable;

    }

    public struct RawMeta
    {
        public bool hasMeta;
        public HtMetaData metaData;
    }

}
