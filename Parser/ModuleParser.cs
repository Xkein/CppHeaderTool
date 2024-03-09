using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Types;
using Scriban.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Parser
{
    internal class ModuleParser : ParserBase
    {
        public string moduleName { get; private set; }
        public List<string> moduleFiles { get; private set; }
        public List<string> includeDirs { get; private set; }
        public List<string> systemIncludeDirs { get; private set; }

        private CppParserOptions _parserOptions;

        public ModuleParser(string moduleName, IEnumerable<string> moduleFiles, IEnumerable<string> includeDirs, IEnumerable<string> systemIncludeDirs)
        {
            this.moduleName = moduleName;
            this.moduleFiles = moduleFiles.ToList();
            this.includeDirs = includeDirs.ToList();
            this.systemIncludeDirs = systemIncludeDirs.ToList();

            _parserOptions = new CppParserOptions()
            {
                ParseAsCpp = true,
                ParseMacros = true,
                AutoSquashTypedef = true,
            };
            _parserOptions.Defines.AddRange(new List<string> {
                "__HEADER_TOOL__",
                "NDEBUG",
            });
            _parserOptions.AdditionalArguments.AddRange(new List<string> {
                "-std=c++20",
            });
            _parserOptions.IncludeFolders.AddRange(includeDirs);
            _parserOptions.SystemIncludeFolders.AddRange(systemIncludeDirs);
        }

        public override async void Parse()
        {
            Task<CppCompilation> compileTask = Task.Run(CompileHeaders);

            if (!ParseMeta())
            {
                Session.hasError = false;
                return;
            }

            CppCompilation compilation = await compileTask;

            if(compilation.HasErrors)
            {
                Session.hasError = false;
                return;
            }

            HtModule htModule = new HtModule();
            htModule.moduleName = moduleName;
            htModule.cppCompilation = compilation;

            ParseChildren(compilation);

            Session.typeTables.Add(htModule);
        }

        private bool ParseMeta()
        {
            return Parallel.ForEach(moduleFiles, file =>
            {
                var parser = new MetaParser(file);
                parser.Parse();
            }).IsCompleted;
        }

        private CppCompilation CompileHeaders()
        {
            CppCompilation compilation = CppParser.ParseFiles(moduleFiles, _parserOptions);
            Session.compilation = compilation;

            foreach (CppDiagnosticMessage message in compilation.Diagnostics.Messages)
            {
                Console.WriteLine(message);
            }

            return compilation;
        }

        private void ParseChildren(CppCompilation compilation)
        {
            foreach (CppEnum cppClass in compilation.Enums)
            {
                var parser = new EnumParser(cppClass);
                parser.Parse();
            }

            foreach (CppClass cppClass in compilation.Classes)
            {
                var parser = new ClassParser(cppClass);
                parser.Parse();
            }

            foreach (CppFunction cppFunction in compilation.Functions)
            {
                var parser = new FunctionParser(cppFunction);
                parser.Parse();
            }

            foreach (CppField cppField in compilation.Fields)
            {
                var parser = new PropertyParser(cppField);
                parser.Parse();
            }
        }
    }
}
