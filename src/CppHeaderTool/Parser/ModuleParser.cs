using CppAst;
using CppHeaderTool.Tables;
using CppHeaderTool.Types;
using Newtonsoft.Json;
using Serilog;
using ShellProgressBar;
using System.Collections.Generic;

namespace CppHeaderTool.Parser
{
    public class ModuleParseInfo
    {
        public string moduleName;
        public string inputText;
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
        private string _inputText;
        public ModuleParser(ModuleParseInfo info)
        {
            _parserOptions = new CppParserOptions()
            {
                ParseTokenAttributes = Session.config.parseTokenAttributes,
                ParseAsCpp = Session.config.parseAsCpp,
                ParseMacros = Session.config.parseMacros,
                AutoSquashTypedef = Session.config.autoSquashTypedef,
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
            _parserOptions.PreHeaderText = Session.config.preHeaderText;
            _parserOptions.PostHeaderText = Session.config.postHeaderText;

            moduleName = info.moduleName;
            moduleFiles = info.moduleFiles.ToList();
            includeDirs = _parserOptions.IncludeFolders;
            systemIncludeDirs = _parserOptions.SystemIncludeFolders;
            defines = _parserOptions.Defines;
            arguments = _parserOptions.AdditionalArguments;
            _inputText = info.inputText;
        }

        static ModuleParser()
        {
            UserCustom.UserParseField += PropertyParser.ParseCursor;
            UserCustom.UserParseClass += ClassParser.ParseCursor;
            UserCustom.UserParseEnum += EnumParser.ParseCursor;
            UserCustom.UserParseEnumItem += EnumParser.ParseCursor;
            UserCustom.UserParseFunction += FunctionParser.ParseCursor;
        }

        protected override string lockerName => moduleName;
        protected override async ValueTask ParseInternal()
        {
            if (Session.typeTables.TryGet(moduleName, out _))
                return;

            Log.Information($"Parsing module {moduleName}");
            Log.Information($"ParserOptions: {JsonConvert.SerializeObject(_parserOptions, Formatting.Indented)}");

            Task<CppCompilation> compileTask = Task.Run(CompileHeaders);
            //Task<CppCompilation[]> compileTask = Task.Run(MultiCompilerHeaders);

            if (!await ParseMeta())
            {
                Session.hasError = true;
                return;
            }

            Log.Information("Waiting compile result...");
            CppCompilation compilation = compileTask.Result;
            //CppCompilation[] compilations = compileTask.Result;

            if (Session.hasError)
            {
                return;
            }

            Log.Information("Parsing compile result...");

            HtModule htModule = new HtModule();
            htModule.moduleName = moduleName;
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

            var pbarOption = new ProgressBarOptions() { DisplayTimeInRealTime = false, ProgressBarOnBottom = true, EnableTaskBarProgress = true };
            using var pbar = new ProgressBar(moduleFiles.Count, "Parsing meta...", pbarOption);
            
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            int errorCount = 0; const int MAX_ERROR_COUNT = 10;
            await Parallel.ForEachAsync(moduleFiles, tokenSource.Token, async (file, token) =>
            {
                try
                {
                    var parser = new MetaParser(file);
                    await parser.Parse();
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Parsing meta error from file {file}");
                    if (Interlocked.Increment(ref errorCount) == MAX_ERROR_COUNT)
                    {
                        Log.Error("too many errors. exiting module parser...");
                        tokenSource.Cancel();
                    }
                }
                finally
                {
                    pbar.Tick();
                }
            });
            Log.Information($"End parsing meta from files in module {moduleName}");

            return true;
        }

        private CppCompilation CompileHeaders()
        {
            Log.Information($"compiling...");
            CppCompilation compilation;
            if (string.IsNullOrEmpty(_inputText))
            {
                compilation = CppParser.ParseFiles(moduleFiles, _parserOptions);
            }
            else
            {
                compilation = CppParser.Parse(_inputText, _parserOptions);
            }

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

            if (compilation.HasErrors)
            {
                Session.hasError = true;
            }

            return compilation;
        }

        private CppCompilation[] MultiCompilerHeaders()
        {
            int batch = Math.Max(Session.config.compileBatch, (int)Math.Ceiling(moduleFiles.Count / (double)Environment.ProcessorCount));
            int count = (int)Math.Ceiling(moduleFiles.Count / (double)batch);
            if (!Session.config.multiThread)
            {
                batch = moduleFiles.Count - 1;
                count = 1;
            }
            CppCompilation[] compilations = new CppCompilation[count];
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Parallel.ForAsync(0, count, tokenSource.Token, (index, token) =>
            {
                int from = index * batch;
                int to = Math.Min(moduleFiles.Count - 1, from + batch - 1);
                Log.Information($"compiling files[{from}..{to}]...");
                CppCompilation compilation = CppParser.ParseFiles(moduleFiles[from..to], _parserOptions);
                Log.Information($"compiled files[{from}..{to}]...");
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
                compilations[index] = compilation;
                if (compilation.HasErrors)
                {
                    Session.hasError = true;
                }
                return ValueTask.CompletedTask;
            }).Wait();
            return compilations;
        }

        private async Task ParseChildren(HtModule htModule, CppCompilation compilation)
        {
            List<CppEnum> enums = compilation.Enums.ToList();
            List<CppClass> classes = compilation.Classes.ToList();
            List<CppFunction> functions = compilation.Functions.ToList();
            List<CppField> fields = compilation.Fields.ToList();
            List<CppNamespace> namespaces = compilation.Namespaces.ToList();
            // collect all namespaces
            for (int idx = 0; idx < namespaces.Count; idx++)
            {
                namespaces.AddRange(namespaces[idx].Namespaces);
            }
            foreach (CppNamespace ns in namespaces)
            {
                enums.AddRange(ns.Enums);
                classes.AddRange(ns.Classes);
                functions.AddRange(ns.Functions);
                fields.AddRange(ns.Fields);
            }
            await Task.WhenAll(
                this.ParseList(enums).AsTask(),
                this.ParseList(classes).AsTask(),
                this.ParseList(functions).AsTask(),
                this.ParseList(fields).AsTask()
            );

            var typeTables = Session.typeTables;

            Log.Information($"collecting parsed types...");
            foreach (CppClass cppClass in classes)
            {
                if (typeTables.TryGet(cppClass, out HtClass htClass))
                {
                    htModule.classes.Add(htClass);
                }
            }
            foreach (CppFunction cppFunction in functions)
            {
                if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                {
                    htModule.functions.Add(htFunction);
                }
            }

            foreach (CppField cppField in fields)
            {
                if (typeTables.TryGet(cppField, out HtProperty htProperty))
                {
                    htModule.properties.Add(htProperty);
                }
            }

            foreach (CppEnum cppEnum in enums)
            {
                if (typeTables.TryGet(cppEnum, out HtEnum htEnum))
                {
                    htModule.enums.Add(htEnum);
                }
            }
        }
        private async Task ParseChildren(HtModule htModule, CppCompilation[] compilations)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            await Parallel.ForEachAsync(compilations, tokenSource.Token, async (compilation, token) =>
            {
                await Task.WhenAll(
                    this.ParseList(compilation.Enums).AsTask(),
                    this.ParseList(compilation.Classes).AsTask(),
                    this.ParseList(compilation.Functions).AsTask(),
                    this.ParseList(compilation.Fields).AsTask()
                );
            });

            var typeTables = Session.typeTables;

            Log.Information($"collecting parsed types...");
            foreach (CppCompilation compilation in compilations)
            {
                Dictionary<string, HtClass> classes = new();
                Dictionary<string, HtFunction> functions = new();
                Dictionary<string, HtProperty> fields = new();
                Dictionary<string, HtEnum> enums = new();
                foreach (CppClass cppClass in compilation.Classes)
                {
                    if (typeTables.TryGet(cppClass, out HtClass htClass))
                    {
                        classes[TypeTables.GetUniqueName(cppClass)] = htClass;
                    }
                }
                foreach (CppFunction cppFunction in compilation.Functions)
                {
                    if (typeTables.TryGet(cppFunction, out HtFunction htFunction))
                    {
                        functions[TypeTables.GetUniqueName(cppFunction)] = htFunction;
                    }
                }
                foreach (CppField cppField in compilation.Fields)
                {
                    if (typeTables.TryGet(cppField, out HtProperty htProperty))
                    {
                        fields[TypeTables.GetUniqueName(cppField)] = htProperty;
                    }
                }
                foreach (CppEnum cppEnum in compilation.Enums)
                {
                    if (typeTables.TryGet(cppEnum, out HtEnum htEnum))
                    {
                        enums[TypeTables.GetUniqueName(cppEnum)] = htEnum;
                    }
                }

                htModule.classes.AddRange(classes.Values);
                htModule.functions.AddRange(functions.Values);
                htModule.properties.AddRange(fields.Values);
                htModule.enums.AddRange(enums.Values);
            }
        }
    }
}
