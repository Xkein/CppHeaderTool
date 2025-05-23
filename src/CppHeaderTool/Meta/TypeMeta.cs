﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Meta
{
    public interface IHasMeta
    {
        ref RawMeta rawMeta { get; }
    }

    public struct ClassMeta
    {
        public AccessibilityMeta Accessibility;
        public ReflectionMeta Reflection;
        public SerializationMeta Serialization;
        public RawMeta Raw;

        // public bool AllVisible;
    }
    public struct EnumConstantMeta
    {
        public RawMeta Raw;

        // public string DisplayName;
    }
    public struct EnumMeta
    {
        public AccessibilityMeta Accessibility;
        public ReflectionMeta Reflection;
        public SerializationMeta Serialization;
        public RawMeta Raw;
    }
    public struct FunctionMeta
    {
        public AccessibilityMeta Accessibility;
        public ReflectionMeta Reflection;
        public SerializationMeta Serialization;
        public RawMeta Raw;

    }
    public struct PropertyMeta
    {
        public AccessibilityMeta Accessibility;
        public ReflectionMeta Reflection;
        public SerializationMeta Serialization;
        public RawMeta Raw;
    }
}
