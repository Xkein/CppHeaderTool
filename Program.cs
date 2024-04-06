
using CommandLine;
using CppHeaderTool.CodeGen;
using CppHeaderTool.Parser;
using Newtonsoft.Json;
using Scriban.Parsing;
using Serilog;
using Serilog.Events;
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

            [Option("log_level")]
            public LogEventLevel logLevel { get; set; } = LogEventLevel.Information;

            [Option("test_generate")]
            public bool testGenerate { get; set; }
        }
        static int Main(string[] args)
        {
            string commandLine = "CppHeaderTool command line: " + string.Join(' ', args);
            ParserResult<CmdLineArgs> parserResult = CommandLine.Parser.Default.ParseArguments<CmdLineArgs>(args);
            if (parserResult.Tag == ParserResultType.NotParsed)
            {
                Console.WriteLine(commandLine);
                return -1;
            }

            CmdLineArgs cmdLineArgs = parserResult.Value;
            string outDir = cmdLineArgs.outDir;
            if (!Directory.Exists(outDir)) {
                Directory.CreateDirectory(outDir);
            }

            CreateLogger(cmdLineArgs);
            Log.Information(commandLine);

            string configPath = cmdLineArgs.config;
            if (!File.Exists(configPath))
            {
                Log.Error($"could not find config {configPath}");
                return -1;
            }
            Session.outDir = outDir;
            Session.config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            Log.Information($"Config: {JsonConvert.SerializeObject(Session.config, Formatting.Indented)}");

            int result = MainTask().Result;

            if (cmdLineArgs.testGenerate)
            {
                TestGenerateTask().Wait();
            }
            return result;
        }

        static void CreateLogger(CmdLineArgs args)
        {
            string logFile = Path.Combine(args.outDir, "CppHeaderTool.Log.txt");
            if (File.Exists(logFile)) {
                File.Delete(logFile);
            }
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logFile)
                .MinimumLevel.Is(args.logLevel)
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
            Stopwatch stopwatch = Stopwatch.StartNew();

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

            stopwatch.Stop();
            Log.Information($"Generate time: {stopwatch.Elapsed}");

            return 0;
        }

        static async Task TestGenerateTask()
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

            while (true)
            {
                Console.WriteLine("press 'x' to exit loop re-generation");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.X)
                {
                    break;
                }
                else
                {
                    try
                    {
                        Session.templateManager.Clear();
                        ModuleCodeGenerator moduleCodeGenerator = new ModuleCodeGenerator(moduleParseInfo.moduleName, Session.outDir);
                        await moduleCodeGenerator.Generate();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
    }

}
