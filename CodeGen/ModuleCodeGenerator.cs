using CppAst;
using CppHeaderTool.Templates;
using CppHeaderTool.Types;
using Scriban.Runtime;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            if (!Session.typeTables.TryGet(moduleName, out _module))
            {
                return;
            }
            CppCompilation compilation = _module.cppCompilation;

            List<Task> genTasks = new List<Task>();

            genTasks.Add(GenerateModuleAsync(_module));

            foreach (CppEnum cppEnum in compilation.Enums)
            {
                if(Session.typeTables.TryGet(cppEnum, out HtEnum htEnum))
                {
                    genTasks.Add(GenerateEnumAsync(htEnum));
                }
            }

            foreach (CppClass cppClass in compilation.Classes)
            {
                if (Session.typeTables.TryGet(cppClass, out HtClass htClass))
                {
                    genTasks.Add(GenerateClassAsync(htClass));
                }
            }

            await Task.WhenAll(genTasks);
        }

        private async Task GenerateModuleAsync(HtModule module)
        {
            await Session.templateManager.Generate(module, new[]
            {
                new TemplateGenerateInfo("module_header.scriban", Path.Combine(Session.outDir, "module", module.moduleName + ".h")),
                new TemplateGenerateInfo("module_cpp.scriban", Path.Combine(Session.outDir, "module", module.moduleName + ".cpp")),
            });
        }

        private async Task GenerateEnumAsync(HtEnum htEnum)
        {
            await Session.templateManager.Generate(htEnum, new[]
            {
                new TemplateGenerateInfo("enum_header.scriban", Path.Combine(Session.outDir, "enum", htEnum.cppEnum.Name + ".h")),
                new TemplateGenerateInfo("enum_cpp.scriban", Path.Combine(Session.outDir, "enum", htEnum.cppEnum.Name + ".cpp")),
            });
        }

        private async Task GenerateClassAsync(HtClass htClass)
        {
            await Session.templateManager.Generate(htClass, new[]
            {
                new TemplateGenerateInfo("class_header.scriban", Path.Combine(Session.outDir, "class", htClass.cppClass.Name + ".h")),
                new TemplateGenerateInfo("class_cpp.scriban", Path.Combine(Session.outDir, "class", htClass.cppClass.Name + ".cpp")),
            });
        }
    }
}
