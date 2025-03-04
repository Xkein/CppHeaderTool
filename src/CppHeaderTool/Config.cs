
using CppAst;

namespace CppHeaderTool
{
    internal class Config
    {
        public string templateDir { get; set; }

        public Dictionary<string, string> moduleTemplates { get; set; }
        public Dictionary<string, string> typeTemplates { get; set; }
        public Dictionary<string, string> injectMetaTemplates { get; set; }

        public string moduleName { get; set; }

        public string inputText { get; set; }

        public string[] headerFiles { get; set; }

        public string[] includeDirs { get; set; }

        public string[] systemIncludeDirs { get; set; }

        public string[] defines { get; set; }

        public string[] arguments { get; set; }

        public bool multiThread { get; set; } = true;
        public int compileBatch { get; set; } = 20;
        public bool parseTokenAttributes { get; set; } = true;
        public bool parseAsCpp { get; set; } = true;
        public bool parseMacros { get; set; } = true;
        public bool autoSquashTypedef { get; set; } = true;

        // CppParserOptions
        public bool isWindowsMsvc { get; set; }
        public bool isParseSystemIncludes { get; set; } = true;
        public string targetAbi { get; set; }
        public string targetSystem { get; set; }
        public string targetVendor { get; set; }
        public string targetCpuSub { get; set; }
        public string targetCpu { get; set; }
        public string preHeaderText { get; set; }
        public string postHeaderText { get; set; }
    }
}
