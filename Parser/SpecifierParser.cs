using CppHeaderTool.Meta;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
using CppHeaderTool.Tokenizer;
using CppHeaderTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Parser
{
    internal class SpecifierParser : ParserBase
    {
        private HtSpecifierContext _specifierContext;
        private HtSpecifierTable _table;

        public SpecifierParser(HtSpecifierContext specifierContext, HtSpecifierTable table)
        {
            _specifierContext = specifierContext;
            _table = table;
        }

        public void Reset(HtSpecifierContext specifierContext, HtSpecifierTable table)
        {
            _specifierContext = specifierContext;
            _table = table;
        }

        public override void Parse()
        {
            HtMetaData metaData = _specifierContext.metaData;
            foreach (string token in metaData.GetSpecifiers())
            {
                if (_table.TryGetValue(token, out HtSpecifier specifier))
                {
                    if (TryParseValue(specifier.type, out object? value, token))
                    {
                        Dispatch(specifier, value);
                    }
                }
            }
        }

        private bool TryParseValue(HtSpecifierType type, out object? value, string token)
        {
            value = null;

            HtMetaData metaData = _specifierContext.metaData;

            switch (type)
            {
                case HtSpecifierType.Tag:

                    return true;
                case HtSpecifierType.String:
                    value = (StringView)metaData.GetString(token);
                    return true;
            }

            return false;
        }

        private void Dispatch(HtSpecifier specifier, object? value)
        {
            specifier.Dispatch(_specifierContext, value);
        }
    }
}
