using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Meta
{
    public struct EnumConstantMeta
    {
        public string DisplayName;
    }
    public struct EnumMeta
    {
        public AccessibilityMeta Accessibility;
        public ReflectionMeta Reflection;
        public SerializationMeta Serialization;
    }
}
