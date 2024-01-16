using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Meta
{
    public struct ClassMeta
    {
        public AccessibilityMeta Accessibility;
        public ReflectionMeta Reflection;
        public SerializationMeta Serialization;

        public bool AllReflected;
    }
}
