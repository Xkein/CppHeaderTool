using CppAst;
using CppHeaderTool.Types;
using Newtonsoft.Json;
using Serilog;
using ShellProgressBar;

namespace CppHeaderTool.Parser
{
    public class ModuleParseInfo
    {
        public string moduleName;
        public IEnumerable<string> moduleFiles;
        public IEnumerable<string> includeDirs;
        public IEnumerable<string> systemIncludeDirs;
        public IEnumerable<string> defines;
        public IEnumerable<string> arguments;
    }
    internal class ModuleParser : ParserBase
    {
        public string moduleName { get; private set; }
        public List<string> moduleFiles { get; private set; }
        public List<string> includeDirs { get; private set; }
        public List<string> systemIncludeDirs { get; private set; }
        public List<string> defines { get; private set; }
        public List<string> arguments { get; private set; }

        private CppParserOptions _parserOptions;

        public ModuleParser(ModuleParseInfo info)
        {
            _parserOptions = new CppParserOptions()
            {
                ParseTokenAttributes = true,
                ParseAsCpp = true,
                ParseMacros = true,
                AutoSquashTypedef = true,
            }.EnableMacros();

            _parserOptions.ParseSystemIncludes = Session.config.isParseSystemIncludes;
            
            if (!string.IsNullOrEmpty(Session.config.targetAbi))
                _parserOptions.TargetAbi = Session.config.targetAbi;

            if (!string.IsNullOrEmpty(Session.config.targetSystem))
                _parserOptions.TargetSystem = Session.config.targetSystem;

            if (!string.IsNullOrEmpty(Session.config.targetVendor))
                _parserOptions.TargetVendor = Session.config.targetVendor;

            if (!string.IsNullOrEmpty(Session.config.targetCpuSub))
                _parserOptions.TargetCpuSub = Session.config.targetCpuSub;

            if (Enum.TryParse(Session.config.targetCpu, out CppTargetCpu targetCpu))
                _parserOptions.TargetCpu = targetCpu;

            if (Session.config.isWindowsMsvc)
                _parserOptions = _parserOptions.ConfigureForWindowsMsvc(_parserOptions.TargetCpu);

            _parserOptions.Defines.AddRange(info.defines);
            _parserOptions.AdditionalArguments.AddRange(info.arguments);
            _parserOptions.IncludeFolders.AddRange(info.includeDirs);
            _parserOptions.SystemIncludeFolders.AddRange(info.systemIncludeDirs);

            moduleName = info.moduleName;
            moduleFiles = info.moduleFiles.ToList();
            includeDirs = _parserOptions.IncludeFolders;
            systemIncludeDirs = _parserOptions.SystemIncludeFolders;
            defines = _parserOptions.Defines;
            arguments = _parserOptions.AdditionalArguments;

        }

        public override async ValueTask Parse()
        {
            Log.Information($"Parsing module {moduleName}");
            Log.Information($"ParserOptions: {JsonConvert.SerializeObject(_parserOptions, Formatting.Indented)}");

            Task<CppCompilation> compileTask = Task.Run(CompileHeaders);

            if (!await ParseMeta())
            {
                Session.hasError = true;
                return;
            }

            Log.Information("Waiting compile result...");
            CppCompilation compilation = compileTask.Result;

            if(compilation.HasErrors)
            {
                Session.hasError = true;
                return;
            }

            HtModule htModule = new HtModule();
            htModule.moduleName = moduleName;
            htModule.cppCompilation = compilation;
            htModule.classes = new List<HtClass>();
            htModule.functions = new List<HtFunction>();
            htModule.properties = new List<HtProperty>();
            htModule.enums = new List<HtEnum>();

            await ParseChildren(htModule, compilation);

            Session.typeTables.Add(htModule);
        }

        private async Task<bool> ParseMeta()
        {
            Log.Information($"Parsing meta from {moduleFiles.Count} file in module {moduleName}...");

            var pbarOption = new ProgressBarOptions() { DisplayTimeInRealTime = false, ProgressBarOnBottom = true };
            using var pbar = new ProgressBar(moduleFiles.Count, "Parsing meta...", pbarOption);
            await Parallel.ForEachAsync(moduleFiles, async (file, token) =>
            {
                var parser = new MetaParser(file);
                await parser.Parse();
                pbar.Tick();
            });
            Log.Information($"Parsed meta from files in module {moduleName}");

            return true;
        }

        private CppCompilation CompileHeaders()
        {
            Log.Information($"compiling...");
            CppCompilation compilation = CppParser.ParseFiles(moduleFiles, _parserOptions);
            Session.compilation = compilation;

            Log.Information("Compiler messages:");
            foreach (CppDiagnosticMessage message in compilation.Diagnostics.Messages)
            {
                string msg = message.ToString();
                switch (message.Type)
                {
                    case CppLogMessageType.Warning:
                        Log.Warning(msg);
                        break;
                    case CppLogMessageType.Error:
                        Log.Error(msg);
                        break;
                    default:
                        Log.Information(msg);
                        break;
                }
            }
            Log.Information("");

            return compilation;
        }

        private async Task ParseChildren(HtModule htModule, CppCompilation compilation)
        {
            await Task.WhenAll(
                this.ParseList(compilation.Enums).AsTask(),
                this.ParseList(compilation.Classes).AsTask(),
                this.ParseList(compilation.Functions).AsTask(),
                this.ParseList(compilation.Fields).AsTask()
            );

            var typeTables = Session.typeTables;

            foreach (CppClass cppClass in compilation.Classes)
            {
                if (typeTables.TryGet(cppClass, out HtClass htClass))
                {
                    htModule.classes.Add(htClass);
                }
            }
            foreach (CppFunction cppFunction in compilation.Functions)
            {
                if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                {
                    htModule.functions.Add(htFunction);
                }
            }

            foreach (CppField cppField in compilation.Fields)
            {
                if (typeTables.TryGet(cppField, out HtProperty htProperty))
                {
                    htModule.properties.Add(htProperty);
                }
            }

            foreach (CppEnum cppEnum in compilation.Enums)
            {
                if (typeTables.TryGet(cppEnum, out HtEnum htEnum))
                {
                    htModule.enums.Add(htEnum);
                }
            }
        }
    }
}
