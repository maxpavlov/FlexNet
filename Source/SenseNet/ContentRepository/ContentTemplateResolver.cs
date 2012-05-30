using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;

namespace SenseNet.ContentRepository
{
    [Obsolete("Use methods via ContentTemplate instead.",true)]
    public class ContentTemplateResolver
    {
        private ContentTemplateResolver() { }

        private static ContentTemplateResolver _instance;
        public static ContentTemplateResolver Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ContentTemplateResolver();

                return _instance;
            }
        }

        public IEnumerable<T> GetTemplatesForType<T>(string contentTypeName) where T : Node
        {
            string path = Repository.ContentTemplateFolderPath + "/" + contentTypeName;
            var templateDir = Node.LoadNode(path) as IFolder;
            if (templateDir != null)
                return templateDir.Children.OfType<T>();
            
            return null;
        }

        public bool HasTemplate(string contentTypeName)
        {
            var templates = GetTemplatesForType<Node>(contentTypeName);

            return (templates != null && templates.Count() > 0);
        }

        public Node GetTemplate(string contentTypeName)
        {
            var templates = GetTemplatesForType<Node>(contentTypeName);

            if (templates != null)
            {
                if (templates.Count() > 1)
                    Logger.WriteWarning("Non-deterministic, anonymous Content Template load for multitemplated Content Type " + contentTypeName);

                return templates.FirstOrDefault();
            }

            return null;
        }

        public Node GetNamedTemplate(string contentTypeName, string templateName)
        {
            var templates = GetTemplatesForType<Node>(contentTypeName);

            var sel = from Node n in templates
                      where (n.Name == templateName)
                      select n;

            return sel.FirstOrDefault();
        }
    }
}
