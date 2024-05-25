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

        public static string GetUniqueName<T>(T element) where T : CppElement, ICppMember 
        {
            if (element is CppType cppType)
                return cppType.FullName;
            else if (element is CppDeclaration cppDeclaration)
                return element.FullParentName + "::" + cppDeclaration.ToString();
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
            return _modules.TryGetValue(moduleName, out type);
        }

        public bool TryGet(CppClass cppClass, out HtClass type)
        {
            return _classes.TryGetValue(GetUniqueName(cppClass), out type);
        }

        public bool TryGet(CppEnum cppEnum, out HtEnum type)
        {
            return _enums.TryGetValue(GetUniqueName(cppEnum), out type);
        }

        public bool TryGet(CppFunction cppFunction, out HtFunction type)
        {
            return _functions.TryGetValue(GetUniqueName(cppFunction), out type);
        }
        public bool TryGet(CppField cppField, out HtProperty type)
        {
            return _properties.TryGetValue(GetUniqueName(cppField), out type);
        }
    }
}
