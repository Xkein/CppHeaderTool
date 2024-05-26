using CppHeaderTool.Meta;
using CppHeaderTool.Parser;
using CppHeaderTool.Tables;
using CppHeaderTool.Tokenizers;
using CppHeaderTool.Types;
using CppHeaderTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CppHeaderTool.Specifies
{
    public class HtSpecifierContext
    {
        public HtType type;
        //public IUhtTokenReader tokenReader { get; private set; }
        public IHtMessageSite messageSite { get; private set; }
        public HtMetaData metaData { get; private set; }

        public HtSpecifierContext(HtParsingScope scope, IHtMessageSite messageSite, HtMetaData metaData)
        {
            type = scope.scopeType;
            //tokenReader = scope.tokenReader;
            this.messageSite = messageSite;
            this.metaData = metaData;
        }

    }

    /// <summary>
    /// The specifier table contains an instance of HtSpecifier which is used to dispatch the parsing of
    /// a specifier to the implementation
    /// </summary>
    public abstract class HtSpecifier
    {

        /// <summary>
        /// Name of the specifier
        /// </summary>
        public string name { get; set; } = String.Empty;

        /// <summary>
        /// Expected value type
        /// </summary>
        public HtSpecifierType type { get; set; }

        /// <summary>
        /// Dispatch an instance of the specifier
        /// </summary>
        /// <param name="specifierContext">Current context</param>
        /// <param name="value">Specifier value</param>
        /// <returns>Results of the dispatch</returns>
        public abstract void Dispatch(HtSpecifierContext specifierContext, object? value);
    }


    /// <summary>
    /// Delegate for a specifier with no value
    /// </summary>
    /// <param name="specifierContext"></param>
    public delegate void HtSpecifierTagDelegate(HtSpecifierContext specifierContext);


    /// <summary>
    /// Specifier delegate with a string value
    /// </summary>
    /// <param name="specifierContext">Specifier context</param>
    /// <param name="value">Specifier value</param>
    public delegate void HtSpecifierStringDelegate(HtSpecifierContext specifierContext, StringView value);



    /// <summary>
    /// Specifier with no value
    /// </summary>
    public class HtSpecifierTag : HtSpecifier
    {
        private readonly HtSpecifierTagDelegate _delegate;

        /// <summary>
        /// Construct the specifier
        /// </summary>
        /// <param name="name">Name of the specifier</param>
        /// <param name="when">When the specifier is executed</param>
        /// <param name="specifierDelegate">Delegate to invoke</param>
        public HtSpecifierTag(string name, HtSpecifierTagDelegate specifierDelegate)
        {
            this.name = name;
            type = HtSpecifierType.Tag;
            _delegate = specifierDelegate;
        }

        /// <inheritdoc/>
        public override void Dispatch(HtSpecifierContext specifierContext, object? value)
        {
            _delegate(specifierContext);
        }
    }

    /// <summary>
    /// Specifier with a string value
    /// </summary>
    public class HtSpecifierString : HtSpecifier
    {
        private readonly HtSpecifierStringDelegate _delegate;

        /// <summary>
        /// Construct the specifier
        /// </summary>
        /// <param name="name">Name of the specifier</param>
        /// <param name="when">When the specifier is executed</param>
        /// <param name="specifierDelegate">Delegate to invoke</param>
        public HtSpecifierString(string name, HtSpecifierStringDelegate specifierDelegate)
        {
            this.name = name;
            type = HtSpecifierType.String;
            _delegate = specifierDelegate;
        }

        /// <inheritdoc/>
        public override void Dispatch(HtSpecifierContext specifierContext, object? value)
        {
            if (value == null)
            {
                throw new Exception("Required value is null");
            }
            _delegate(specifierContext, (StringView)value);
        }
    }

}
