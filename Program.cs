
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

            Task<int> task = MainTask();
            return task.Result;
        }

        static async Task<int> MainTask()
        {
            string[][] files = await Task.WhenAll(
                File.ReadAllLinesAsync(Session.config.source),
                File.ReadAllLinesAsync(Session.config.include),
                File.ReadAllLinesAsync(Session.config.systemInclude)
            );
            string[] srcFiles = files[1];
            string[] includeFiles = files[2];
            string[] systemIncludeFiles = files[3];

            ModuleParser moduleParser = new ModuleParser(Session.config.module, srcFiles, includeFiles, systemIncludeFiles);
            await moduleParser.Parse();

            if (Session.hasError)
            {
                return -1; 
            }

            ModuleCodeGenerator moduleCodeGenerator = new ModuleCodeGenerator(Session.config.module, Session.config.outDir);
            await moduleCodeGenerator.Generate();

            if (Session.hasError)
            {
                return -1;
            }

            return 0;
        }
    }

}
