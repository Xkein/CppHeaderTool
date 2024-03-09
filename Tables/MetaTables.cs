using CppHeaderTool.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Tables
{

    internal class FileMetaTables : HtLookupTable<int, HtMetaData>
    {

    }
    internal class MetaTables : HtLookupTables<FileMetaTables>
    {
        public MetaTables() : base("meta")
        {
        }
    }
}
