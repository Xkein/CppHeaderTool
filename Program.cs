
using CommandLine;
using CppHeaderTool.CodeGen;
using CppHeaderTool.Parser;
using Scriban.Runtime.Accessors;
using Serilog;
using System.Diagnostics;

namespace CppHeaderTool
{
    public class Program
    {
        static int Main(string[] args)
        {
            string commandLine = "CppHeaderTool command line: " + string.Join(' ', args);
            ParserResult<Config> result = CommandLine.Parser.Default.ParseArguments<Config>(args).WithParsed(config =>
            {
                Session.config = config;
            });
            if (result.Tag == ParserResultType.NotParsed)
            {
                Console.WriteLine(commandLine);
                return -1;
            }

            CreateLogger();
            Log.Information(commandLine);

            Task<int> task = MainTask();
            return task.Result;
        }

        static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(Session.config.outDir, "CppHeaderTool.Log.txt"))
                .CreateLogger();
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
                ReadFileAsync(Session.config.systemInclude),
                ReadFileAsync(Session.config.defines),
                ReadFileAsync(Session.config.arguments)
            );
            string[] srcFiles = files[0];
            string[] includeFiles = files[1];
            string[] systemIncludeFiles = files[2];
            string[] defines = files[3];
            string[] arguments = files[4];

            ModuleParser moduleParser = new ModuleParser(Session.config.module, srcFiles, includeFiles, systemIncludeFiles, defines, arguments);
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
