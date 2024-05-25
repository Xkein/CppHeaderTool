using CppAst;
using CppHeaderTool.Tables;
using CppHeaderTool.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool
{
    internal class Session
    {
        public static Config config { get; set; }

        public static HtTables tables { get; set; } = new HtTables();
        public static TypeTables typeTables => tables.typeTables;
        public static MetaTables metaTables => tables.metaTables;
        //public static SpecifierTables specifierTables => tables.specifierTables;
        
        public static TemplateManager templateManager { get; } = new TemplateManager();

        public static bool hasError { get; set; }
        public static string outDir { get; set; }
    }
}
