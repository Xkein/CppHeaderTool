using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Templates
{
    internal class CodeTemplate
    {
        public string templateText;
        public Template template;


        public void ReadTemplate(string filePath)
        {
            Console.WriteLine($"reading code template {filePath}");
            templateText = File.ReadAllText(filePath);
            template = Template.Parse(templateText);
        }

        public string Render(TemplateContext context)
        {
            return template.Render(context);
        }
    }
}
