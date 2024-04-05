
using CppAst;

namespace CppHeaderTool
{
    internal class Config
    {
        public string templateDir { get; set; }

        public string moduleName { get; set; }

        public string[] headerFiles { get; set; }

        public string[] includeDirs { get; set; }

        public string[] systemIncludeDirs { get; set; }

        public string[] defines { get; set; }

        public string[] arguments { get; set; }

        // CppParserOptions
        public bool isWindowsMsvc { get; set; }
        public string targetAbi { get; set; }
        public string targetSystem { get; set; }
        public string targetVendor { get; set; }
        public string targetCpuSub { get; set; }
        public string targetCpu { get; set; }
    }
}
