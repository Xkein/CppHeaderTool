using CommandLine;
using CppHeaderTool.CodeGen;
using CppHeaderTool.Parser;
using CppHeaderTool.Tables;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

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

            [Option("save_parser_result", HelpText = "WIP")]
            public bool saveParserResult { get; set; }

            [Option("read_cached_parser_result", HelpText = "WIP")]
            public bool readCachedParserResult { get; set; }

            // for test
            [Option("loop_generate")]
            public bool loopGenerate { get; set; }
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

            int result = MainTask(cmdLineArgs).Result;

            if (cmdLineArgs.loopGenerate)
            {
                LoopGenerateTask().Wait();
            }
            return result;
        }

        static void CreateLogger(CmdLineArgs args)
        {
            string logFile = Path.Combine(args.outDir, "CppHeaderTool.log");
            if (File.Exists(logFile)) {
                File.Delete(logFile);
            }
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(logFile)
                .MinimumLevel.Is(args.logLevel)
                .CreateLogger();
        }

        static string sCachedFilePath => Path.Combine(Session.outDir, "parser_cache");

        static async Task<int> MainTask(CmdLineArgs args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            bool isUseCache = false;
            if (args.readCachedParserResult)
            {
                try
                {
                    Log.Information($"reading parser cache from file: {sCachedFilePath}");

                    ParserCacheResult.Load(sCachedFilePath);
                    isUseCache = true;
                }
                catch (Exception e)
                {
                    Log.Error(e, "could not load parser result from cache!");
                }
            }

            if (!isUseCache)
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

                if (args.saveParserResult)
                {
                    try
                    {
                        Log.Information($"saving parser result to file: {sCachedFilePath}");
                        ParserCacheResult.Save(sCachedFilePath);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "could not save parser result cache!");
                    }
                }
            }

            if (Session.hasError)
            {
                return -1; 
            }

            ModuleCodeGenerator moduleCodeGenerator = new ModuleCodeGenerator(Session.config.moduleName, Session.outDir);
            await moduleCodeGenerator.Generate();

            if (Session.hasError)
            {
                return -1;
            }

            stopwatch.Stop();
            Log.Information($"Generate time: {stopwatch.Elapsed}");

            return 0;
        }

        static async Task LoopGenerateTask()
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
                Console.WriteLine("press 'X' to exit loop re-generation, 'R' to re-parse code");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.X)
                {
                    break;
                }
                else if (key.Key == ConsoleKey.R)
                {
                    Session.tables = new();
                    ModuleParser moduleParser = new ModuleParser(moduleParseInfo);
                    await moduleParser.Parse();
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
