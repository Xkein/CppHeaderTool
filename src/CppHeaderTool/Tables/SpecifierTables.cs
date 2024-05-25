using CppHeaderTool.Specifies;
using CppHeaderTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{
    public enum HtSpecifierType
    {
        Tag,
        String,

    }
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class HtSpecifierAttribute : Attribute
    {
        public HtSpecifierAttribute(string keyword)
        {
            Keyword = keyword;
        }
        public string Name { get; set; }
        public string Keyword { get; }
        public HtSpecifierType ValueType { get; set; } = HtSpecifierType.Tag;
    }

    internal class HtSpecifierTable : HtLookupTable<HtSpecifier>
    {
        public HtSpecifierTable Add(HtSpecifier specifier)
        {
            base.Add(specifier.name, specifier);
            return this;
        }
    }

    internal class SpecifierTables : HtLookupTables<HtSpecifierTable>
    {

        public SpecifierTables() : base("specifiers")
        {
        }



        public void OnSpecifierAttribute(Type type, MethodInfo methodInfo, HtSpecifierAttribute specifierAttribute)
        {
            string name = GetSpecifierName(methodInfo, specifierAttribute);

            if (String.IsNullOrEmpty(specifierAttribute.Keyword))
            {
                throw new Exception($"The 'Specifier' attribute on the {type.Name}.{methodInfo.Name} method doesn't have a table specified.");
            }

            HtSpecifierTable table = Get(specifierAttribute.Keyword);
            switch (specifierAttribute.ValueType)
            {
                case HtSpecifierType.Tag:
                    table.Add(new HtSpecifierTag(name, (HtSpecifierTagDelegate)Delegate.CreateDelegate(typeof(HtSpecifierTagDelegate), methodInfo)));
                    break;
                case HtSpecifierType.String:
                    table.Add(new HtSpecifierString(name, (HtSpecifierStringDelegate)Delegate.CreateDelegate(typeof(HtSpecifierStringDelegate), methodInfo)));
                    break;
            }
        }

        private string GetSpecifierName(MethodInfo methodInfo, HtSpecifierAttribute specifierAttribute)
        {
            string name = specifierAttribute.Name;
            if (String.IsNullOrEmpty(name))
            {
                name = methodInfo.Name;
            }
            return name;
        }
    }
}
