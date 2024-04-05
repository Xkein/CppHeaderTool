﻿using CppAst;
using CppHeaderTool.Specifies;
using CppHeaderTool.Tables;
using CppHeaderTool.Types;
using Scriban.Parsing;
using Serilog;
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
        public List<string> defines { get; private set; }
        public List<string> arguments { get; private set; }

        private CppParserOptions _parserOptions;

        public ModuleParser(
            string moduleName,
            IEnumerable<string> moduleFiles,
            IEnumerable<string> includeDirs,
            IEnumerable<string> systemIncludeDirs,
            IEnumerable<string> defines,
            IEnumerable<string> arguments)
        {
            this.moduleName = moduleName;
            this.moduleFiles = moduleFiles.ToList();
            this.includeDirs = includeDirs.ToList();
            this.systemIncludeDirs = systemIncludeDirs.ToList();
            this.defines = defines.ToList();
            this.arguments = arguments.ToList();

            _parserOptions = new CppParserOptions()
            {
                ParseTokenAttributes = true,
                ParseAsCpp = true,
                ParseMacros = true,
                AutoSquashTypedef = true,
            };
            _parserOptions.Defines.AddRange(defines);
            _parserOptions.AdditionalArguments.AddRange(arguments);
            _parserOptions.IncludeFolders.AddRange(includeDirs);
            _parserOptions.SystemIncludeFolders.AddRange(systemIncludeDirs);
        }

        public override async ValueTask Parse()
        {
            Log.Information($"Parsing module {moduleName} with info:");
            Log.Information($"moduleFiles: {string.Join(", ", moduleFiles)}");
            Log.Information($"includeDirs: {string.Join(", ", includeDirs)}");
            Log.Information($"systemIncludeDirs: {string.Join(", ", systemIncludeDirs)}");
            Log.Information($"defines: {string.Join(", ", defines)}");
            Log.Information($"arguments: {string.Join(", ", arguments)}");

            Task<CppCompilation> compileTask = Task.Run(CompileHeaders);

            if (!await ParseMeta())
            {
                Session.hasError = true;
                return;
            }

            CppCompilation compilation = compileTask.Result;

            if(compilation.HasErrors)
            {
                Session.hasError = true;
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

            Log.Information($"CppCompilation input text:{Environment.NewLine}{compilation.InputText}");
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
