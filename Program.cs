
using CommandLine;
using CppHeaderTool.CodeGen;
using CppHeaderTool.Parser;
using Scriban.Runtime.Accessors;
using System.Diagnostics;

namespace CppHeaderTool
{
    public class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("CppHeaderTool command line: " + string.Join(' ', args));
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

        static async Task<string[]> ReadFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                return new string[0];
            }

            return await File.ReadAllLinesAsync(path);
        }

        static async Task<int> MainTask()
        {
            string[][] files = await Task.WhenAll(
                ReadFileAsync(Session.config.source),
                ReadFileAsync(Session.config.include),
                ReadFileAsync(Session.config.systemInclude)
            );
            string[] srcFiles = files[0];
            string[] includeFiles = files[1];
            string[] systemIncludeFiles = files[2];

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
