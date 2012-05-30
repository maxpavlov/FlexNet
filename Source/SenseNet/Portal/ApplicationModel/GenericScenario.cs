using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Versioning;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.Portal.Virtualization;
using SenseNet.Search;

namespace SenseNet.ApplicationModel
{
    public class GenericScenario
    {
        public virtual string Name
        {
            get; set;
        }

        public virtual IComparer<ActionBase> GetActionComparer()
        {
            return new ActionComparer();
        }

        public IEnumerable<ActionBase> GetActions(Content context, string backUrl)
        {
            var actions = CollectActions(context, backUrl).ToList();

            FilterWithRequiredPermissions(actions, context);

            var comparer = GetActionComparer();
            if (comparer != null) actions.Sort(comparer);

            return actions;
        }

        protected virtual IEnumerable<ActionBase> CollectActions(Content context, string backUrl)
        {
            //customize action list in derived classes
            return ActionFramework.GetActions(context, this.Name, backUrl);
        }

        public virtual void Initialize(Dictionary<string, object> parameters)
        {
            //consume custom parameters in the derived class
        }

        public static ServiceAction GetServiceAction(Content context, string backUrl, string methodName, string title, string iconName, int index)
        {
            var act = ActionFramework.GetAction("ServiceAction", context, backUrl, new { path = context.Path }) as ServiceAction;

            if (act != null)
            {
                act.Name = methodName;
                act.ServiceName = "ContentStore.mvc";
                act.MethodName = methodName;
                act.Text = title;
                act.Icon = iconName;
                act.Index = index;
            }

            return act;
        }

        public static IEnumerable<Node> GetNewItemNodes(GenericContent content) 
        {
            //  It would be fine to use a list of all available ContentTemplate folders.
            //var result = content.AllowedChildTypes;
            var result = content.GetAllowedChildTypes();
            return GetNewItemsInternal(content, result);
        }

        public static IEnumerable<Node> GetNewItemNodes(GenericContent content, ContentType[] contentTypes) 
        {
            var result = contentTypes;
            return GetNewItemsInternal(content, result);
        }

        private static IEnumerable<Node> GetNewItemsInternal(GenericContent content, IEnumerable<Node> result) {
            var items = new List<Node>();

            // add available content types
            items.AddRange(result);
            
            var currentWorkspace = Workspace.GetWorkspaceForNode(content);
            var wsTemplatePath = currentWorkspace == null ? string.Empty : RepositoryPath.Combine(currentWorkspace.Path, Repository.ContentTemplatesFolderName);
            var siteTemplatePath = RepositoryPath.Combine(PortalContext.Current.Site.Path, Repository.ContentTemplatesFolderName);
            var currentContentTemplatePath = RepositoryPath.Combine(content.Path, Repository.ContentTemplatesFolderName);

            var queryText = new StringBuilder();

            //add filter for workspace templates
            if (!string.IsNullOrEmpty(wsTemplatePath))
                queryText.AppendFormat(" Path:\"{0}\"", wsTemplatePath);

            //add filter for site, current content and global template folders
            queryText.AppendFormat(" Path:\"{0}\" Path:\"{1}\" Path:\"{2}\"", siteTemplatePath, currentContentTemplatePath, Repository.ContentTemplateFolderPath);

            var templateResult = ContentQuery.Query(queryText.ToString(), new QuerySettings { EnableAutofilters =  false, EnableLifespanFilter = false});
            var cloneItems = result.ToArray();

            //clone the list to avoid duplicated Node loads during multiple enumerations below
            var templateFolders = templateResult.Nodes.ToArray();

            // Add local and global content templates to items collection. 
            // The order of the probing is important: bottom --> top          
            ProcessContentTemplateFolder(items, templateFolders.FirstOrDefault(t => t.Path.Equals(currentContentTemplatePath)), cloneItems);
            ProcessContentTemplateFolder(items, templateFolders.FirstOrDefault(t => t.Path.Equals(wsTemplatePath)), cloneItems);
            ProcessContentTemplateFolder(items, templateFolders.FirstOrDefault(t => t.Path.Equals(siteTemplatePath)), cloneItems);
            ProcessContentTemplateFolder(items, templateFolders.FirstOrDefault(t => t.Path.Equals(Repository.ContentTemplateFolderPath)), cloneItems);

            //remove duplicated templates
            foreach (var templateFolder in templateFolders)
            {
                foreach (var cloneItem in cloneItems)
                {
                    if (ContentTemplate.HasTemplate(cloneItem.Name, templateFolder.Path))
                        items.Remove(cloneItem);
                }
            }

            return items;
        }

        private static void ProcessContentTemplateFolder(List<Node> items, Node ctFolder, IEnumerable<Node> cloneItems)
        {
            if (ctFolder == null)
                return;

            var folderQuery = new StringBuilder();

            foreach (var item in cloneItems)
            {
                folderQuery.AppendFormat(" InFolder:\"{0}\"", RepositoryPath.Combine(ctFolder.Path, item.Name));
            }

            var queryText = folderQuery.ToString();
            if (queryText.Length <= 0)
                return;

            var itemNodes = ContentQuery.Query(queryText, new QuerySettings { EnableAutofilters = false, EnableLifespanFilter = false }).Nodes;
            foreach (var templateItem in itemNodes)
            {
                AddItem(items, templateItem);
            }
        }

        private static void AddItem(List<Node> items, Node child)
        {
            if (child == null)
                throw new ArgumentNullException("child");


            var hasItem = from t in items.AsEnumerable()
                          where t.Name.Equals(child.Name) && !(t is ContentType)
                          select t;
            if (hasItem.Count() > 0)
                items.Remove(child);
            else
                items.Add(child);
        }
        
        private static void FilterWithRequiredPermissions(IEnumerable<ActionBase> actions, Content context)
        {
            foreach (var action in actions)
            {
                ActionFramework.CheckRequiredPermissions(action, context);
            }
        }
    }

    public class MyScenarioConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return TypeHandler.CreateInstance(value.ToString());
        }
    } 
}
