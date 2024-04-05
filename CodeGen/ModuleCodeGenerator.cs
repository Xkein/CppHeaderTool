using CppAst;
using CppHeaderTool.Templates;
using CppHeaderTool.Types;
using Serilog;
using ShellProgressBar;
using System;

namespace CppHeaderTool.CodeGen
{
    internal class ModuleCodeGenerator
    {
        public string moduleName { get; private set; }
        public string outDir { get; private set; }

        private HtModule _module;

        public ModuleCodeGenerator(string moduleName, string outDir)
        {
            this.moduleName = moduleName;
            this.outDir = outDir;
        }

        public async Task Generate()
        {
            Log.Information($"Generating module {moduleName}");
            if (!Session.typeTables.TryGet(moduleName, out _module))
            {
                return;
            }
            CppCompilation compilation = _module.cppCompilation;

            List<TemplateGenerateInfo> generateInfos = new List<TemplateGenerateInfo>(100);

            AddGenerateInfo(generateInfos, _module, "module_header.scriban", $"module/{_module.moduleName}.h");
            AddGenerateInfo(generateInfos, _module, "module_cpp.scriban",    $"module/{_module.moduleName}.cpp");

            foreach (CppClass cppClass in compilation.Classes)
            {
                if (Session.typeTables.TryGet(cppClass, out HtClass htClass))
                {
                    AddGenerateInfo(generateInfos, htClass, "class_header.scriban", $"class/{htClass.cppClass.Name}.h");
                    AddGenerateInfo(generateInfos, htClass, "class_cpp.scriban",    $"class/{htClass.cppClass.Name}.cpp");
                }
            }

            foreach (CppEnum cppEnum in compilation.Enums)
            {
                if (Session.typeTables.TryGet(cppEnum, out HtEnum htEnum))
                {
                    AddGenerateInfo(generateInfos, htEnum, "enum_header.scriban", $"enum/{htEnum.cppEnum.Name}.h");
                    AddGenerateInfo(generateInfos, htEnum, "enum_cpp.scriban",    $"enum/{htEnum.cppEnum.Name}.cpp");
                }
            }

            Log.Information($"Generating {generateInfos.Count} code file in module {moduleName}...");

            var pbarOption = new ProgressBarOptions() { DisplayTimeInRealTime = false, ProgressBarOnBottom = true };
            using var pbar = new ProgressBar(generateInfos.Count, "Generating code from template...", pbarOption);
            await Parallel.ForEachAsync(generateInfos, async (TemplateGenerateInfo info, CancellationToken token) =>
            {
                await Session.templateManager.Generate(info);
                pbar.Tick();
            });

            Log.Information($"Generated module {moduleName}");
        }

        private void AddGenerateInfo(List<TemplateGenerateInfo> list, object importObject, string template, string outputPath)
        {
            list.Add(new TemplateGenerateInfo(importObject, template, Path.Combine(Session.outDir, outputPath)));
        }
    }
}
