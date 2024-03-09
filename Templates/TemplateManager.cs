using CppHeaderTool.Types;
using Scriban.Runtime;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Templates
{
    struct TemplateGenerateInfo
    {
        public string template;
        public string outputPath;

        public TemplateGenerateInfo(string template, string outputPath)
        {
            this.template = template;
            this.outputPath = outputPath;
        }
    }

    internal class TemplateManager
    {
        Dictionary<string, CodeTemplate> _templates;

        public TemplateManager()
        {
        
        }

        public CodeTemplate GetTemplate(string path)
        {
            if (!_templates.TryGetValue(path, out CodeTemplate template))
            {
                template = new CodeTemplate();
                template.ReadTemplate(Path.Combine(Session.config.template, path));
                _templates[path] = template;
            }

            return template;
        }

        public async Task Generate(object importObject, TemplateGenerateInfo[] infos)
        {
            var templateContext = new TemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.Import(importObject);

            templateContext.PushGlobal(scriptObject);

            List<Task> tasks = new List<Task>();
            foreach (TemplateGenerateInfo info in infos)
            {
                CodeTemplate template = this.GetTemplate(info.template);
                Task task = WriteTemplateAsync(template, templateContext, info.outputPath);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        private async static Task WriteTemplateAsync(CodeTemplate template, TemplateContext context, string path)
        {
            string content = template.Render(context);
            await File.WriteAllTextAsync(path, content);
        }
    }
}
