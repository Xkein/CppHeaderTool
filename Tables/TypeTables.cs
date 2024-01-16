using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{
    internal class TypeTables
    {
        private Dictionary<CppClass, HtClass> _classes = new();
        private Dictionary<CppEnum, HtEnum> _enums = new();
        private Dictionary<CppFunction, HtFunction> _functions = new();
        private Dictionary<CppField, HtProperty> _properties = new();

        public void Add(HtClass type)
        {
            _classes.Add(type.cppClass, type);
        }

        public void Add(HtEnum type)
        {
            _enums.Add(type.cppEnum, type);
        }

        public void Add(HtFunction type)
        {
            _functions.Add(type.cppFunction, type);
        }

        public void Add(HtProperty type)
        {
            _properties.Add(type.cppField, type);
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
