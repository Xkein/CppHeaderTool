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
        private ConcurrentDictionary<CppClass, HtClass> _classes = new();
        private ConcurrentDictionary<CppEnum, HtEnum> _enums = new();
        private ConcurrentDictionary<CppFunction, HtFunction> _functions = new();
        private ConcurrentDictionary<CppField, HtProperty> _properties = new();

        public void Add(HtModule type)
        {
            _modules.TryAdd(type.moduleName, type);
        }

        public void Add(HtClass type)
        {
            _classes.TryAdd(type.cppClass, type);
        }

        public void Add(HtEnum type)
        {
            _enums.TryAdd(type.cppEnum, type);
        }

        public void Add(HtFunction type)
        {
            _functions.TryAdd(type.cppFunction, type);
        }

        public void Add(HtProperty type)
        {
            _properties.TryAdd(type.cppField, type);
        }

        public bool TryGet(string moduleName, out HtModule type)
        {
            return _modules.TryGetValue(moduleName, out type);
        }

        public bool TryGet(CppClass cppClass, out HtClass type)
        {
            return _classes.TryGetValue(cppClass, out type);
        }

        public bool TryGet(CppEnum cppEnum, out HtEnum type)
        {
            return _enums.TryGetValue(cppEnum, out type);
        }

        public bool TryGet(CppFunction cppFunction, out HtFunction type)
        {
            return _functions.TryGetValue(cppFunction, out type);
        }
        public bool TryGet(CppField cppField, out HtProperty type)
        {
            return _properties.TryGetValue(cppField, out type);
        }
    }
}
