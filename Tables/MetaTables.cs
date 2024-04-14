using CppHeaderTool.Meta;
using CppHeaderTool.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{

    internal class FileMetaTables : ConcurrentDictionary<int, HtMetaData>
    {

    }
    internal class MetaTables : ConcurrentDictionary<string, FileMetaTables>
    {

    }
}
