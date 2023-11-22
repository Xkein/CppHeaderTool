using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{
    public static class HtTableNames
    {
        public const string Default = "Default";
        public const string Global = "Global";
        public const string Class = "Class";
        public const string Enum = "Enum";
        public const string Function = "Function";
        public const string Interface = "Interface";
        public const string Field = "Field";
    }

    public class HtLookupTableBase
    {
        public virtual void Merge(HtLookupTableBase baseTable) { }
    }


    public class HtLookupTable<TValue> : HtLookupTableBase
    {
        private readonly Dictionary<string, TValue> _lookup;

        public HtLookupTable()
        {
            _lookup = new();
        }

        public HtLookupTable<TValue> Add(string key, TValue value)
        {
            _lookup.Add(key, value);
            return this;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue val)
        {
            return _lookup.TryGetValue(key, out val);
        }

        public override void Merge(HtLookupTableBase baseTable)
        {
            foreach (KeyValuePair<string, TValue> pair in ((HtLookupTable<TValue>)baseTable)._lookup)
            {
                _lookup.TryAdd(pair.Key, pair.Value);   
            }
        }
    }

    public class HtTables
    {

    }
}
