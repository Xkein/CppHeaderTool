using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Utils
{

    /// <summary>
    /// A message site is any object that can generate a message.  In general, all 
    /// types are also message sites. This provides a convenient method to log messages
    /// where the type was defined.
    /// </summary>
    public interface IHtMessageSite
    {
        /// <summary>
        /// Source file generating messages
        /// </summary>
        public IUhtMessageSource? MessageSource { get; }
    }

    /// <summary>
    /// A message source represents the source file where the message occurred.
    /// </summary>
    public interface IUhtMessageSource
    {
        /// <summary>
        /// File path of the file being parsed
        /// </summary>
        string MessageFilePath { get; }

        /// <summary>
        /// The full file path of being parsed
        /// </summary>
        string MessageFullFilePath { get; }
    }

}
