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
        public CppCompilation compilation { get; private set; }

        public ModuleCodeGenerator(string moduleName, string outDir, CppCompilation compilation)
        {
            this.moduleName = moduleName;
            this.outDir = outDir;
            this.compilation = compilation;
        }

        public void Generate()
        {
            List<Task> genTasks = new List<Task>();

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
                    genTasks.Add(GenerateClass(htClass));
                }
            }

            Task.WhenAll(genTasks);
        }

        private async Task GenerateEnumAsync(HtEnum htEnum)
        {
            CodeTemplate headerTemplate = Session.templateManager.GetTemplate("enum_header.scriban");
            CodeTemplate cppTemplate = Session.templateManager.GetTemplate("enum_cpp.scriban");

            var templateContext = new TemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.Import(htEnum);

            templateContext.PushGlobal(scriptObject);

            Task headerTask = WriteTemplateAsync(headerTemplate, templateContext, Path.Combine(Session.config.outDir, "class", htEnum.cppEnum.Name + ".h"));
            Task cppTask = WriteTemplateAsync(cppTemplate, templateContext, Path.Combine(Session.config.outDir, "class", htEnum.cppEnum.Name + ".cpp"));
            await Task.WhenAll(headerTask, cppTask);
        }

        private async Task GenerateClass(HtClass htClass)
        {
            CodeTemplate headerTemplate = Session.templateManager.GetTemplate("class_header.scriban");
            CodeTemplate cppTemplate = Session.templateManager.GetTemplate("class_cpp.scriban");

            var templateContext = new TemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.Import(htClass);

            templateContext.PushGlobal(scriptObject);

            Task headerTask = WriteTemplateAsync(headerTemplate, templateContext, Path.Combine(Session.config.outDir, "class", htClass.cppClass.Name + ".h"));
            Task cppTask = WriteTemplateAsync(cppTemplate, templateContext, Path.Combine(Session.config.outDir, "class", htClass.cppClass.Name + ".cpp"));
            await Task.WhenAll(headerTask, cppTask);
        }

        private async Task WriteTemplateAsync(CodeTemplate template, TemplateContext context, string path)
        {
            string content = template.Render(context);
            await File.WriteAllTextAsync(path, content);
        }
    }
}
