using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository;
using SenseNet.Portal.Workspaces;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Search;

namespace SenseNet.Portal.Portlets
{
    public class JournalPortlet : ContextBoundPortlet
    {
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebDisplayName("Max visible items")]
        [WebDescription("Set the maximum number of visible items")]
        [WebOrder(100)]
        public int MaxItems { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Show system content")]
        [WebDescription("If checked the portlet will display the changes of system content too")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        public bool ShowSystemContent { get; set; }

        public JournalPortlet()
        {
            this.Name = "Journal";
            this.Description = "This portlet shows the journal of the related content tree (XSLT only) (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.System);
        }

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            Controls.Clear();

            ChildControlsCreated = true;
        }

        protected override object GetModel()
        {
            var node = GetContextNode();
            if (node == null)
                return null;

            var top = this.MaxItems < 1 ? 10 : this.MaxItems;
            var nodeList = new List<Node>();
            IEnumerable<JournalItem> items;

            if (ShowSystemContent)
            {
                items = Journals.Get(node.Path, top);

                nodeList.AddRange(items.Select(item => new JournalNode(node, item)).Cast<Node>());
            }
            else
            {
                var tempTop = top;
                var tempSkip = 0;

                while (true)
                {
                    items = Journals.Get(node.Path, tempTop, tempSkip, true);
                    var query = new StringBuilder("+(");
                    var pathAdded = false;

                    foreach (var item in items)
                    {
                        query.AppendFormat("Path:\"{0}\" ", item.Wherewith);
                        pathAdded = true;
                    }

                    //not found any journal items, finish the search
                    if (!pathAdded)
                        break;

                    query.Append(") +IsSystemContent:yes");

                    var queryResults = ContentQuery.Query(query.ToString(), new QuerySettings() { EnableAutofilters = false, EnableLifespanFilter = false });
                    var pathList = queryResults.Nodes.Select(n => n.Path).ToArray();

                    var maxToAdd = Math.Max(0, top - nodeList.Count);

                    nodeList.AddRange(items.Where(item => !pathList.Contains(item.Wherewith)).Take(maxToAdd).Select(ji => new JournalNode(node, ji)));

                    if (nodeList.Count >= top || items.Count() == 0)
                        break;

                    tempSkip += tempTop;
                    tempTop = Math.Max(tempTop*2, 50);
                }
            }

            var folder = SearchFolder.Create(nodeList);
            return folder.GetXml(PortalActionLinkResolver.Instance, true);
        }

    }
}
