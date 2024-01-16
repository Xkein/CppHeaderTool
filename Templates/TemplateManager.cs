using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppHeaderTool.Templates
{
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
    }
}
