
using CommandLine;
using CppHeaderTool.CodeGen;
using CppHeaderTool.Parser;
using Newtonsoft.Json;
using Scriban.Parsing;
using Serilog;
using System.Diagnostics;

namespace CppHeaderTool
{
    public class Program
    {
        class CmdLineArgs
        {
            [Option("config", Required = true)]
            public string config { get; set; }

            [Option("out_dir", Required = true)]
            public string outDir { get; set; }
        }
        static int Main(string[] args)
        {
            string commandLine = "CppHeaderTool command line: " + string.Join(' ', args);
            ParserResult<CmdLineArgs> result = CommandLine.Parser.Default.ParseArguments<CmdLineArgs>(args);
            if (result.Tag == ParserResultType.NotParsed)
            {
                Console.WriteLine(commandLine);
                return -1;
            }

            CmdLineArgs cmdLineArgs = result.Value;
            string outDir = cmdLineArgs.outDir;
            if (!Directory.Exists(outDir)) {
                Directory.CreateDirectory(outDir);
            }
            string configPath = cmdLineArgs.config;
            if (!File.Exists(configPath))
            {
                Log.Error($"could not find config {configPath}");
            }
            Session.outDir = outDir;
            Session.config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));

            CreateLogger();
            Log.Information(commandLine);

            Task<int> task = MainTask();
            return task.Result;
        }

        static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(Session.outDir, "CppHeaderTool.Log.txt"))
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
            ModuleParseInfo moduleParseInfo = new ModuleParseInfo()
            {
                moduleName = Session.config.moduleName,
                moduleFiles = Session.config.headerFiles,
                includeDirs = Session.config.includeDirs,
                systemIncludeDirs = Session.config.systemIncludeDirs,
                defines = Session.config.defines,
                arguments = Session.config.arguments,
            };

            ModuleParser moduleParser = new ModuleParser(moduleParseInfo);
            await moduleParser.Parse();

            if (Session.hasError)
            {
                return -1; 
            }

            ModuleCodeGenerator moduleCodeGenerator = new ModuleCodeGenerator(moduleParseInfo.moduleName, Session.outDir);
            await moduleCodeGenerator.Generate();

            if (Session.hasError)
            {
                return -1;
            }

            return 0;
        }
    }

}
