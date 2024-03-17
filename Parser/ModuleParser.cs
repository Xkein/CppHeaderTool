﻿using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
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

        public override async ValueTask Parse()
        {
            Task<CppCompilation> compileTask = Task.Run(CompileHeaders);

            if (!await ParseMeta())
            {
                Session.hasError = false;
                return;
            }

            CppCompilation compilation = compileTask.Result;

            if(compilation.HasErrors)
            {
                Session.hasError = false;
                return;
            }

            HtModule htModule = new HtModule();
            htModule.moduleName = moduleName;
            htModule.cppCompilation = compilation;

            await ParseChildren(htModule, compilation);

            Session.typeTables.Add(htModule);
        }

        private async Task<bool> ParseMeta()
        {
            await Parallel.ForEachAsync(moduleFiles, async (file, token) =>
            {
                var parser = new MetaParser(file);
                await parser.Parse();
            });

            return true;
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
