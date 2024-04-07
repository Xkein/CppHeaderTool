using CppAst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Meta
{
    internal enum SpecifierValueType
    {
        Tag,
        String,
        OptionalString,
        StringList,
    }

    public class HtMetaData
    {
        public string keyword;
        public CppSourceSpan sourceSpan;

        private Dictionary<string, bool> _tags = new ();
        private Dictionary<string, string> _kvPairs = new();
        //private List<List<string>> _stringLists;


        private string _keyword;
        private CppSourceSpan _sourceSpan;
        public HtMetaData(string keyword, CppSourceSpan span)
        {
            _keyword = keyword;
            _sourceSpan = span;
        }

        public void AddTag(string key)
        {
            _tags.Add(key, true);
        }
        public bool GetTag(string key)
        {
            return _tags.ContainsKey(key);
        }

        public void AddKeyValue(string key, string value)
        {
            _kvPairs.Add(key, value);
        }


        public string GetString(string key, string defValue = "")
        {
            return _kvPairs.GetValueOrDefault(key, defValue);
        }

        public string GetOptionalString(string key, string defValue)
        {
            if (_tags.ContainsKey(key))
            {
                return "true";
            }

            if (_kvPairs.TryGetValue(key, out string val))
            {
                return val;
            }

            return defValue;
        }

        public bool GetOptionalBool(string key, bool defValue)
        {
            string val = GetOptionalString(key, defValue ? "true" : "false");
            return val == "true";
        }

        public IEnumerable<string> GetSpecifiers()
        {
            return _tags.Keys.Concat(_kvPairs.Keys);
        }
    }
}
