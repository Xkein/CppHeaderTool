using CommandLine;

namespace CppHeaderTool
{
    internal class Config
    {
        [Option("template", Required = true)]
        public string template { get; set; }

        [Option("module", Required = true)]
        public string module { get; set; }

        [Option("source", Required = true)]
        public string source { get; set; }

        [Option("include")]
        public string include { get; set; }

        [Option("sys_include")]
        public string systemInclude { get; set; }


        [Option("out_dir", Required = true)]
        public string outDir { get; set; }
    }
}
