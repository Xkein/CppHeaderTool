
using CommandLine;
using CppHeaderTool.CodeGen;
using CppHeaderTool.Parser;
using System.Diagnostics;

namespace CppHeaderTool
{
    public class Program
    {
        static int Main(string[] args)
        {
            ParserResult<Config> result = CommandLine.Parser.Default.ParseArguments<Config>(args).WithParsed(config =>
            {
                Session.config = config;
            });

            if (result.Tag == ParserResultType.NotParsed)
            {
                return -1;
            }

            string[] srcFiles = File.ReadAllLines(Session.config.source);
            string[] includeFiles = File.ReadAllLines(Session.config.include);
            string[] systemIncludeFiles = File.ReadAllLines(Session.config.systemInclude);

            ModuleParser moduleParser = new ModuleParser(Session.config.module, srcFiles, includeFiles, systemIncludeFiles);
            moduleParser.Parse();

            if (Session.hasError)
            {
                return -1;
            }

            ModuleCodeGenerator moduleCodeGenerator = new ModuleCodeGenerator(Session.config.module, Session.config.outDir, Session.compilation);
            moduleCodeGenerator.Generate();

            if (Session.hasError)
            {
                return -1;
            }

            return 0;
        }
    }

}
