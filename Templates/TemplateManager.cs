﻿using CppHeaderTool.Types;
using Scriban.Runtime;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scriban.Parsing;
using Serilog;
using System.Threading;

namespace CppHeaderTool.Templates
{
    struct TemplateGenerateInfo
    {
        public object importObject;
        public string template;
        public string outputPath;

        public TemplateGenerateInfo(object importObject, string template, string outputPath)
        {
            this.importObject = importObject;
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
                try
                {
                    templateText = await File.ReadAllTextAsync(filePath);
                    template = Template.Parse(templateText);
                    if (template.HasErrors)
                    {
                        Log.Error($"template error from path: {filePath}");
                        foreach (LogMessage message in template.Messages)
                        {
                            string msg = message.ToString();
                            switch (message.Type)
                            {
                                case ParserMessageType.Error:
                                    Log.Error(msg);
                                    break;
                                case ParserMessageType.Warning:
                                    Log.Warning(msg);
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "error loading code template!");
                    throw;
                }
            }

            public string Render(TemplateContext context)
            {
                try
                {
                    return template.Render(context);
                }
                catch (Exception e)
                {
                    Log.Error(e, "error rendering code template!");
                    return e.ToString();
                }
            }
        }

        SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private async ValueTask<CodeTemplate> GetTemplateAsync(string path)
        {
            if (_templates.TryGetValue(path, out CodeTemplate template))
            {
                return template;
            }

            await _semaphoreSlim.WaitAsync();
            try
            {
                if (_templates.TryGetValue(path, out template))
                {
                    return template;
                }
                template = new CodeTemplate();
                await template.ReadTemplate(Path.Combine(Session.config.templateDir, path));
                _templates[path] = template;
            }
            finally
            {
                _semaphoreSlim.Release();
            }

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


        public async Task Generate(TemplateGenerateInfo info)
        {
            var templateContext = new TemplateContext();
            var scriptObject = new ScriptObject();
            scriptObject.Import(info.importObject);

            templateContext.TemplateLoader = this;
            templateContext.PushGlobal(scriptObject);

            CodeTemplate template = await GetTemplateAsync(info.template);
            await WriteTemplateAsync(template, templateContext, info.outputPath);
        }

        private async static ValueTask WriteTemplateAsync(CodeTemplate template, TemplateContext context, string path)
        {
            string content = template.Render(context);
            bool isFileExist = File.Exists(path);
            if (string.IsNullOrEmpty(content))
            {
                if (isFileExist)
                {
                    File.Delete(path);
                }
            }
            else
            {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string oldContent = "";
                if (isFileExist) {
                    oldContent = await File.ReadAllTextAsync(path);
                }
                if (oldContent != content)
                {
                    await File.WriteAllTextAsync(path, content);
                }
                Log.Information($"{path} generated!");
            }
        }
    }
}
