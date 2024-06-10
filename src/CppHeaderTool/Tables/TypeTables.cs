using ClangSharp.Interop;
using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{
    internal class TypeTables
    {
        private ConcurrentDictionary<string, HtModule> _modules = new();
        private ConcurrentDictionary<string, HtClass> _classes = new();
        private ConcurrentDictionary<string, HtEnum> _enums = new();
        private ConcurrentDictionary<string, HtFunction> _functions = new();
        private ConcurrentDictionary<string, HtProperty> _properties = new();

        private static string GetUniqueName(CppType cppType)
        {
            if (cppType is CppClass cppClass)
            {
                if (cppClass.IsAnonymous)
                {
                    return cppType.FullParentName + "::AnonymousClass~" + cppClass.GetHashCode();
                }
            }
            return cppType.FullName;
        }

        private static string GetUniqueName(CppDeclaration cppDeclaration)
        {
            if (cppDeclaration is CppField cppField)
            {
                if (cppField.IsAnonymous)
                {
                    return cppField.FullParentName + "::AnonymousField~" + cppField.GetHashCode();
                }
            }
            return cppDeclaration.FullParentName + "::" + cppDeclaration.ToString();
        }

        public static string GetUniqueName<T>(T element) where T : CppElement, ICppMember 
        {
            if (element is CppType)
                return GetUniqueName(element as CppType);
            else if (element is CppDeclaration)
                return GetUniqueName(element as CppDeclaration);
            return element.FullParentName + "::" + element.Name;
        }

        public void Add(HtModule type)
        {
            _modules.TryAdd(type.moduleName, type);
        }

        public void Add(HtClass type)
        {
            _classes.TryAdd(GetUniqueName(type.cppClass), type);
        }

        public void Add(HtEnum type)
        {
            _enums.TryAdd(GetUniqueName(type.cppEnum), type);
        }

        public void Add(HtFunction type)
        {
            _functions.TryAdd(GetUniqueName(type.cppFunction), type);
        }

        public void Add(HtProperty type)
        {
            _properties.TryAdd(GetUniqueName(type.cppField), type);
        }

        public bool TryGet(string moduleName, out HtModule type)
        {
            return TryGetModule(moduleName, out type);
        }

        public bool TryGet(CppClass cppClass, out HtClass type)
        {
            return TryGetClass(GetUniqueName(cppClass), out type);
        }

        public bool TryGet(CppEnum cppEnum, out HtEnum type)
        {
            return TryGetEnum(GetUniqueName(cppEnum), out type);
        }

        public bool TryGet(CppFunction cppFunction, out HtFunction type)
        {
            return TryGetFunction(GetUniqueName(cppFunction), out type);
        }
        public bool TryGet(CppField cppField, out HtProperty type)
        {
            return TryGetProperty(GetUniqueName(cppField), out type);
        }

        public bool TryGetModule(string name, out HtModule module)
        {
            return _modules.TryGetValue(name, out module);
        }

        public bool TryGetClass(string name, out HtClass type)
        {
            return _classes.TryGetValue(name, out type);
        }

        public bool TryGetEnum(string name, out HtEnum type)
        {
            return _enums.TryGetValue(name, out type);
        }

        public bool TryGetFunction(string name, out HtFunction type)
        {
            return _functions.TryGetValue(name, out type);
        }
        public bool TryGetProperty(string name, out HtProperty type)
        {
            return _properties.TryGetValue(name, out type);
        }
    }
}
