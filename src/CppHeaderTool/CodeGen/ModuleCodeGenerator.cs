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


            List<TemplateGenerateInfo> injectInfos = new List<TemplateGenerateInfo>(1);
            Log.Information($"Injecting extra meta for module {moduleName}");
            AddGenerateInfos(injectInfos, Session.config.injectMetaTemplates, _module, _module.moduleName);
            Task injectTask = RunGenerateTask(injectInfos);

            List<TemplateGenerateInfo> generateInfos = new List<TemplateGenerateInfo>(100);
            AddGenerateInfos(generateInfos, Session.config.moduleTemplates, _module, _module.moduleName);

            foreach (HtClass htClass in _module.classes)
            {
                AddGenerateInfos(generateInfos, Session.config.typeTemplates, htClass, htClass.cppClass.Name);
            }

            foreach (HtEnum htEnum in _module.enums)
            {
                AddGenerateInfos(generateInfos, Session.config.typeTemplates, htEnum, htEnum.cppEnum.Name);
            }

            await injectTask;
            if (Session.hasError) {
                Log.Error($"error when injecting meta in module {moduleName}");
                return;
            }
            Log.Information($"Generating {generateInfos.Count} code file in module {moduleName}...");
            await RunGenerateTask(generateInfos);
            Log.Information($"Generated module {moduleName}");
        }

        private async Task RunGenerateTask(List<TemplateGenerateInfo> generateInfos)
        {
            var pbarOption = new ProgressBarOptions() { DisplayTimeInRealTime = false, ProgressBarOnBottom = true, EnableTaskBarProgress = true };
            using var pbar = new ProgressBar(generateInfos.Count, "Generating code from template...", pbarOption);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            int errorCount = 0; const int MAX_ERROR_COUNT = 10;
            await Parallel.ForEachAsync(generateInfos, tokenSource.Token, async (TemplateGenerateInfo info, CancellationToken token) =>
            {
                try
                {
                    await Session.templateManager.Generate(info);
                }
                catch (Exception e)
                {
                    Session.hasError = true;
                    Log.Error(e, $"error generating {info.outputPath}");
                    if (Interlocked.Increment(ref errorCount) == MAX_ERROR_COUNT)
                    {
                        Log.Error("too many errors. exiting module generator...");
                        tokenSource.Cancel();
                    }
                }
                finally
                {
                    pbar.Tick();
                }
            });
        }

        private void AddGenerateInfos(List<TemplateGenerateInfo> list, Dictionary<string, string> templates, object importObject, string name)
        {
            foreach (var (template, outPath) in templates)
            {
                AddGenerateInfo(list, importObject, template, string.Format(outPath, name, moduleName));
            }
        }

        private void AddGenerateInfo(List<TemplateGenerateInfo> list, object importObject, string template, string outputPath)
        {
            list.Add(new TemplateGenerateInfo(importObject, template, Path.Combine(Session.outDir, outputPath), _module));
        }
    }
}
