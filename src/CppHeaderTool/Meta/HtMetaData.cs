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
        public string keyword => _keyword;
        public CppSourceSpan sourceSpan => _sourceSpan;

        public Dictionary<string, bool> tags => _tags;
        public Dictionary<string, string> kvPairs => _kvPairs;
        public Dictionary<string, string[]> stringList => _stringList;

        private Dictionary<string, bool> _tags = new ();
        private Dictionary<string, string> _kvPairs = new();
        private Dictionary<string, string[]> _stringList = new();


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

        public void AddStringList(string key, string[] list)
        {
            _stringList.Add(key, list);
        }

        public string GetString(string key, string defValue = "")
        {
            return _kvPairs.GetValueOrDefault(key, defValue);
        }

        public string[] GetStringList(string key)
        {
            return _stringList.GetValueOrDefault(key, []);
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"tags: [{string.Join(", ", tags.Keys)}]; ");
            sb.Append($"pairs: [{string.Join(", ", kvPairs.Select(p => $"({p.Key}: {p.Value})"))}]; ");
            sb.Append($"string list: {{ {string.Join(", ", stringList.Select(p => $"{p.Key}: [{string.Join(", ", p.Value)}]"))} }}; ");
            return sb.ToString();
        }
    }
}
