using CppHeaderTool.Types;
using Scriban.Runtime;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scriban.Parsing;
using Serilog;

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

    internal class TemplateManager : ITemplateLoader
    {
        public TemplateManager()
        {
        
        }

        Dictionary<string, CodeTemplate> _templates = new();
        class CodeTemplate
        {
            public string templateText;
            public Template template;


            public async Task ReadTemplate(string filePath)
            {
                Log.Information($"loading code template {filePath}");
                templateText = await File.ReadAllTextAsync(filePath);
                template = Template.Parse(templateText);
            }

            public string Render(TemplateContext context)
            {
                return template.Render(context);
            }
        }

        private async ValueTask<CodeTemplate> GetTemplateAsync(string path)
        {
            if (_templates.TryGetValue(path, out CodeTemplate template))
            {
                return template;
            }

            template = new CodeTemplate();
            await template.ReadTemplate(Path.Combine(Session.config.templateDir, path));
            _templates[path] = template;

            return template;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            return templateName;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            CodeTemplate codeTemplate = GetTemplateAsync(templatePath).Result;
            return codeTemplate.templateText;
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            var task = GetTemplateAsync(templatePath);
            if (task.IsCompletedSuccessfully)
            {
                return task.Result.templateText;
            }

            await task;
            return task.Result.templateText;
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
                CodeTemplate template = await GetTemplateAsync(info.template);
                ValueTask task = WriteTemplateAsync(template, templateContext, info.outputPath);
                if (!task.IsCompletedSuccessfully)
                {
                    tasks.Add(task.AsTask());
                }
            }
            await Task.WhenAll(tasks);
        }

        private async static ValueTask WriteTemplateAsync(CodeTemplate template, TemplateContext context, string path)
        {
            string content = template.Render(context);
            if (!string.IsNullOrEmpty(content))
            {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                await File.WriteAllTextAsync(path, content);
                Log.Information($"{path} generated!");
            }
        }
    }
}
