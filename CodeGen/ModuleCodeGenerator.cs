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

            AddGenerateInfos(generateInfos, Session.config.moduleTemplates, _module, _module.moduleName);

            foreach (CppClass cppClass in compilation.Classes)
            {
                if (Session.typeTables.TryGet(cppClass, out HtClass htClass))
                {
                    AddGenerateInfos(generateInfos, Session.config.classTemplates, htClass, htClass.cppClass.Name);
                }
            }

            foreach (CppEnum cppEnum in compilation.Enums)
            {
                if (Session.typeTables.TryGet(cppEnum, out HtEnum htEnum))
                {
                    AddGenerateInfos(generateInfos, Session.config.enumTemplates, htEnum, htEnum.cppEnum.Name);
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

        private void AddGenerateInfos(List<TemplateGenerateInfo> list, Dictionary<string, string> templates, object importObject, string name)
        {
            foreach (var (template, outPath) in templates)
            {
                AddGenerateInfo(list, importObject, template, string.Format(outPath, name));
            }
        }

        private void AddGenerateInfo(List<TemplateGenerateInfo> list, object importObject, string template, string outputPath)
        {
            list.Add(new TemplateGenerateInfo(importObject, template, Path.Combine(Session.outDir, outputPath)));
        }
    }
}
