using CppHeaderTool.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{
    /// <summary>
    /// Base class for table lookup system.
    /// </summary>
    public class HtLookupTableBase
    {
        /// <summary>
        /// This table inherits entries for the given table
        /// </summary>
        public HtLookupTableBase? ParentTable { get; set; } = null;

        /// <summary>
        /// Name of the table
        /// </summary>
        public string TableName { get; set; } = String.Empty;

        /// <summary>
        /// User facing name of the table
        /// </summary>
        public string UserName
        {
            get => String.IsNullOrEmpty(_userName) ? TableName : _userName;
            set => _userName = value;
        }

        /// <summary>
        /// Check to see if the table is internal
        /// </summary>
        public bool Internal { get; set; } = false;

        /// <summary>
        /// If true, this table has been implemented and not just created on demand by another table
        /// </summary>
        public bool Implemented { get; set; } = false;

        /// <summary>
        /// Internal version of the user name.  If it hasn't been set, then the table name will be used
        /// </summary>
        private string _userName = String.Empty;

        /// <summary>
        /// Merge the lookup table.  Duplicates will be ignored.
        /// </summary>
        /// <param name="baseTable">Base table being merged</param>
        public virtual void Merge(HtLookupTableBase baseTable)
        {
        }

        /// <summary>
        /// Given a method name, try to extract the entry name for a table
        /// </summary>
        /// <param name="classType">Class containing the method</param>
        /// <param name="methodInfo">Method information</param>
        /// <param name="inName">Optional name supplied by the attributes.  If specified, this name will be returned instead of extracted from the method name</param>
        /// <param name="suffix">Required suffix</param>
        /// <returns>The extracted name or the supplied name</returns>
        public static string GetSuffixedName(Type classType, MethodInfo methodInfo, string? inName, string suffix)
        {
            string name = inName ?? String.Empty;
            if (String.IsNullOrEmpty(name))
            {
                if (methodInfo.Name.EndsWith(suffix, StringComparison.Ordinal))
                {
                    name = methodInfo.Name[..^suffix.Length];
                }
                else
                {
                    throw new Exception($"The '{suffix}' attribute on the {classType.Name}.{methodInfo.Name} method doesn't have a name specified or the method name doesn't end in '{suffix}'.");
                }
            }
            return name;
        }
    }

    /// <summary>
    /// Lookup tables provide a method of associating actions with given C++ keyword or UE specifier
    /// </summary>
    /// <typeparam name="TValue">Keyword or specifier information</typeparam>
    public class HtLookupTable<TValue> : HtLookupTableBase
    {

        /// <summary>
        /// Lookup dictionary for the specifiers
        /// </summary>
        private readonly Dictionary<StringView, TValue> _lookup;

        public HtLookupTable() : this(StringViewComparer.OrdinalIgnoreCase)
        {
        }
        /// <summary>
        /// Construct a new table
        /// </summary>
        public HtLookupTable(StringViewComparer comparer)
        {
            _lookup = new Dictionary<StringView, TValue>(comparer);
        }

        /// <summary>
        /// Add the given value to the lookup table.  It will throw an exception if it is a duplicate.
        /// </summary>
        /// <param name="key">Key to be added</param>
        /// <param name="value">Value to be added</param>
        public HtLookupTable<TValue> Add(string key, TValue value)
        {
            _lookup.Add(key, value);
            return this;
        }

        /// <summary>
        /// Attempt to fetch the value associated with the key
        /// </summary>
        /// <param name="key">Lookup key</param>
        /// <param name="value">Value associated with the key</param>
        /// <returns>True if the key was found, false if not</returns>
        public bool TryGetValue(StringView key, [MaybeNullWhen(false)] out TValue value)
        {
            return _lookup.TryGetValue(key, out value);
        }

        /// <summary>
        /// Merge the lookup table.  Duplicates will be ignored.
        /// </summary>
        /// <param name="baseTable">Base table being merged</param>
        public override void Merge(HtLookupTableBase baseTable)
        {
            foreach (KeyValuePair<StringView, TValue> kvp in ((HtLookupTable<TValue>)baseTable)._lookup)
            {
                _lookup.TryAdd(kvp.Key, kvp.Value);
            }
        }
    }

    public class HtLookupTable<TKey, TValue> : HtLookupTableBase
    {

        /// <summary>
        /// Lookup dictionary for the specifiers
        /// </summary>
        private readonly Dictionary<TKey, TValue> _lookup;

        public HtLookupTable() : this(null)
        {
        }
        /// <summary>
        /// Construct a new table
        /// </summary>
        public HtLookupTable(IEqualityComparer<TKey> comparer)
        {
            _lookup = new Dictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        /// Add the given value to the lookup table.  It will throw an exception if it is a duplicate.
        /// </summary>
        /// <param name="key">Key to be added</param>
        /// <param name="value">Value to be added</param>
        public HtLookupTable<TKey, TValue> Add(TKey key, TValue value)
        {
            _lookup.Add(key, value);
            return this;
        }

        /// <summary>
        /// Attempt to fetch the value associated with the key
        /// </summary>
        /// <param name="key">Lookup key</param>
        /// <param name="value">Value associated with the key</param>
        /// <returns>True if the key was found, false if not</returns>
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _lookup.TryGetValue(key, out value);
        }

        /// <summary>
        /// Merge the lookup table.  Duplicates will be ignored.
        /// </summary>
        /// <param name="baseTable">Base table being merged</param>
        public override void Merge(HtLookupTableBase baseTable)
        {
            foreach (KeyValuePair<TKey, TValue> kvp in ((HtLookupTable<TKey, TValue>)baseTable)._lookup)
            {
                _lookup.TryAdd(kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// A collection of lookup tables by name.
    /// </summary>
    /// <typeparam name="TTable">Table type</typeparam>
    public class HtLookupTables<TTable> where TTable : HtLookupTableBase, new()
    {

        /// <summary>
        /// Collection of named tables
        /// </summary>
        public Dictionary<string, TTable> Tables { get; } = new Dictionary<string, TTable>();

        /// <summary>
        /// The name of the group of tables
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Create a new group of tables
        /// </summary>
        /// <param name="name">The name of the group</param>
        public HtLookupTables(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Given a table name, return the table.  If not found, a new one will be added with the given name.
        /// </summary>
        /// <param name="tableName">The name of the table to return</param>
        /// <returns>The table associated with the name.</returns>
        public TTable Get(string tableName)
        {
            if (!Tables.TryGetValue(tableName, out TTable? table))
            {
                table = new TTable();
                table.TableName = tableName;
                Tables.Add(tableName, table);
            }
            return table;
        }

        /// <summary>
        /// Create a table with the given information.  If the table already exists, it will be initialized with the given data.
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="userName">The user facing name of the name</param>
        /// <param name="parentTableName">The parent table name used to merge table entries</param>
        /// <param name="tableIsInternal">If true, the table is internal and won't be visible to the user.</param>
        /// <returns>The created table</returns>
        public TTable Create(string tableName, string userName, string? parentTableName, bool tableIsInternal = false)
        {
            TTable table = Get(tableName);
            table.UserName = userName;
            table.Internal = tableIsInternal;
            table.Implemented = true;
            if (!String.IsNullOrEmpty(parentTableName))
            {
                table.ParentTable = Get(parentTableName);
            }
            return table;
        }

        /// <summary>
        /// Merge the contents of all parent tables into their children.  This is done so that the 
        /// parent chain need not be searched when looking for table entries.
        /// </summary>
        /// <exception cref="UhtIceException">Thrown if there are problems with the tables.</exception>
        public void Merge()
        {
            List<TTable> orderedList = new(Tables.Count);
            List<TTable> remainingList = new(Tables.Count);
            HashSet<HtLookupTableBase> doneTables = new();

            // Collect all of the tables
            foreach (KeyValuePair<string, TTable> kvp in Tables)
            {
                if (!kvp.Value.Implemented)
                {
                    throw new Exception($"{Name} table '{kvp.Value.TableName}' has been referenced but not implemented");
                }
                remainingList.Add(kvp.Value);
            }

            // Perform a topological sort of the tables
            while (remainingList.Count != 0)
            {
                bool addedOne = false;
                for (int i = 0; i < remainingList.Count;)
                {
                    TTable table = remainingList[i];
                    if (table.ParentTable == null || doneTables.Contains(table.ParentTable))
                    {
                        orderedList.Add(table);
                        doneTables.Add(table);
                        remainingList[i] = remainingList[^1];
                        remainingList.RemoveAt(remainingList.Count - 1);
                        addedOne = true;
                    }
                    else
                    {
                        ++i;
                    }
                }
                if (!addedOne)
                {
                    throw new Exception($"Circular dependency in {GetType().Name}.{Name} tables detected");
                }
            }

            // Merge the tables
            foreach (TTable table in orderedList)
            {
                if (table.ParentTable != null)
                {
                    table.Merge((TTable)table.ParentTable);
                }
            }
        }
    }
}
