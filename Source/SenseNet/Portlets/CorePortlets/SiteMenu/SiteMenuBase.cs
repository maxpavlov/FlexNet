using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.Web.UI;
using SenseNet.Diagnostics;
using SenseNet.Search;

namespace SenseNet.Portal.Portlets
{
    public static class ObjectExtensions
    {
        public static XPathNavigator ToXPathNavigator(this object obj)
        {
            var serializer = new XmlSerializer(obj.GetType());
            var ms = new MemoryStream();
            serializer.Serialize(ms, obj); 
            ms.Position = 0;
            var doc = new XPathDocument(ms);

            return doc.CreateNavigator();
        }
    }

    public abstract class SiteMenuBase : ContextBoundPortlet
    {
        #region Properties
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Depth")]
        [WebDescription("Number of levels to display")]
        [DefaultValue(1)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(10)]
        public int Depth { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Show hidden")]
        [WebDescription("Set it to false to hide pages and content marked as 'Hidden'")]
        [DefaultValue(false)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(20)]
        public bool ShowHiddenPages { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Visible content types")]
        [WebDescription("Comma separated string of visible content type names")]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(30)]
        public string ShowTypeNames { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Show pages only")]
        [WebDescription("Set this to true to show pages only in the menu")]
        [DefaultValue(true)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(40)]
        public bool ShowPagesOnly { get; set; }

        [WebBrowsable(false)]
        [Personalizable(true)]
        [DefaultValue(true)]
        [Obsolete("Use OmitContextNode instead")]
        public bool EmitContextNode { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Hide target content")]
        [WebDescription("Set this to false to show the target content in the menu")]
        [DefaultValue(true)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(50)]
        public bool OmitContextNode { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Expand menu to context")]
        [WebDescription("")]
        [DefaultValue(true)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(60)]
        public bool ExpandToContext { get; set; }
             
        private bool _loadFullTree = true;

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Load full tree")]
        [WebDescription("When set to true the whole tree from the selected target is shown in the given depth")]
        [DefaultValue(true)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(70)]
        public bool LoadFullTree
        {
            get { return _loadFullTree; }
            set { _loadFullTree = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Get context children")]
        [WebDescription("When set to true the direct children of the selected target is shown")]
        [DefaultValue(true)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(80)]
        public bool GetContextChildren { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Menu CSS class")]
        [WebDescription("Css class of the rendered menu. Default is 'sn-menu'")]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [WebOrder(90)]
        public string PortletCssClass { get; set; }

        [WebDisplayName("Children filter")]
        [WebDescription("Optional filter for the children query of menu items")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.SiteMenu, EditorCategory.SiteMenu_Order)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MiddleSize)]
        [WebOrder(100)]
        public string QueryFilter { get; set; }

        protected NavigableNodeFeed Feed { get; set; }

        #endregion

        protected override XsltArgumentList GetXsltArgumentList()
        {
            var arguments = base.GetXsltArgumentList() ?? new XsltArgumentList();
            new[] 
                {
                    new {Name = "CurrentSite" , Value = (object)PortalContext.Current.Site as Node},
                    new {Name = "CurrentPage" , Value = (object)PortalContext.Current.Page as Node},
                    new {Name = "CurrentUser" , Value = (object)User.Current as Node },
                    new {Name = "CurrentContext" , Value = (object)PortalContext.Current.ContextNode as Node }
                }.Select(node =>
                {
                    arguments.AddParam(node.Name, string.Empty,
                        new SenseNet.Services.ContentStore.Content(node.Value).ToXPathNavigator());
                    return true;
                }).ToArray();

            new[]
                    {
                     new { Namespace = "urn:sn:hu", Value = (object)new NodeQueryXsltProxy() },
                     new { Namespace = "sn://SenseNet.ContentRepository.i18n.ResourceXsltClient", Value = (object)new ResourceXsltClient() }
                    }.Select(ext =>
                    {
                        arguments.AddExtensionObject(ext.Namespace, ext.Value); return true;
                    }
              ).ToArray();
            return arguments;
        }
        private string[] _typeNames;
        private bool _typesInitialized;

        private string[] TypeNames
        {
            get
            {
                if (!_typesInitialized)
                {
                    if (!string.IsNullOrEmpty(ShowTypeNames))
                        _typeNames = ShowTypeNames.Split(new[] { ',' }).Select(s => s.Trim()).ToArray();
                    _typesInitialized = true;
                }
                return _typeNames;
            }
        }
        public override RenderMode RenderingMode
        {
            get
            {
                //BREAK: Phasing out phase-out logic
                //if (this.RenderMode != default(RenderMode))
                //{
                //    this.RenderingMode = this.RenderMode;
                //    this.RenderMode = default(RenderMode);
                //}
                return base.RenderingMode;
            }
            set
            {
                //BREAK: Phasing out phase-out logic
                //this.RenderMode = default(RenderMode);
                base.RenderingMode = value;
            }
        }
        
        protected override object GetModel()
        {
            if (Feed == null)
            {
                if (BindTarget == BindTarget.Breadcrumb)
                    Feed = ConvertNodesToFeed(GetParentsNodes());
                else
                    Feed = ConvertNodesToFeed(GetNavigableNodes());
                Feed.PortletCssClass = String.IsNullOrEmpty(PortletCssClass) ? "sn-menu" : PortletCssClass;
            }

            return Feed;
        }

        protected virtual Node[] GetParentsNodes()
        {
            var nodeList = new List<Node>();
            var current = GetBindingRoot();
            while (current != null)
            {
                Node page;
                if (ShowPagesOnly)
                    page = current as Page;
                else
                    page = current as GenericContent;
                if (page != null)
                    nodeList.Add(current);
                current = current.Parent;
            }
            return nodeList.ToArray();
        }

        internal class TempControl : UserControl
        {
            public NavigableNodeFeed Feed { get; set; }
            protected override void Render(HtmlTextWriter writer)
            {
                Feed.Render(writer);
                //base.Render(writer);
            }            
        }

        protected override void CreateChildControls()
        {
            using (var traceOperation = Logger.TraceOperation("SiteMenuBase.CreateChildControls"))
            {
                if (CanCache && Cacheable && IsInCache)
                    return;

                base.CreateChildControls();
                var feed = GetModel() as NavigableNodeFeed;
                if (feed == null)
                {
                    if (this.RenderException != null)
                        this.Controls.Add(new LiteralControl("Portlet error: " + this.RenderException.Message));
                    else
                        throw new InvalidDataException("Model data is invalid");
                }
                else
                {
                    this.Controls.Add(new TempControl {Feed = feed});
                }

                traceOperation.IsSuccessful = true;
            }
        }

        protected virtual Node[] GetNavigableNodes()
        {
            var depth = Depth > 0 ? (OmitContextNode ? Depth + 1 : Depth) : 3;
            var enumeratorContext = LoadFullTree ? null : PortalContext.Current.ContextNodePath;

            if (ContextNode == null)
                return new Node[0];

            try
            {
                IEnumerable<Node> nodes = null;
                if (!string.IsNullOrEmpty(this.QueryFilter))
                {
                    nodes = SiteMenuNodeEnumerator.GetNodes(ContextNode.Path, ExecutionHint.None, this.QueryFilter, depth,
                                                            enumeratorContext, GetContextChildren).ToList();
                }
                else
                {
                    nodes = SiteMenuNodeEnumerator.GetNodes(ContextNode.Path, ExecutionHint.None, GetQuery(), depth,
                                                            enumeratorContext, GetContextChildren).ToList();
                }

                return nodes.Where(node => NodeIsValid(node)).ToArray();   
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                return new Node[0];
            }
        }

        private static bool PathIsTopLevel(string p, string[] allp)
        {
            return allp.Count(path => p != path && p.StartsWith(path)) == 0;
        }

        protected virtual NavigableNodeFeed ConvertNodesToFeed(Node[] nodes)
        {
            var allPaths = nodes.Select(node => node.Path).ToArray();
            var topPaths = allPaths.Where(path => PathIsTopLevel(path, allPaths)).ToArray();
            var relativeRoots = nodes.Where(node => topPaths.Contains(node.Path)).ToArray();
            var treeNodes = relativeRoots.Select((node, index) => BuildUpTreeNodes(node, nodes, index, 0))
                .OrderBy(node => node.Index).ToArray();

            if (OmitContextNode && treeNodes.Count() > 0)
                treeNodes = treeNodes[0].Nodes;

            return new NavigableNodeFeed { Nodes = treeNodes };
        }

        protected virtual NavigableTreeNode BuildUpTreeNodes(Node relativeParent, Node[] allItems, int index, int level)
        {
            var treeNode = new NavigableTreeNode(relativeParent, 
                PortalContext.Current.Site.Path, 
                PortalContext.Current.ContextNode.Path)
                {
                    Level = level,
                    PhysicalIndex = index + 1
                };

            treeNode.Nodes =
                allItems.Where(node => ((node != null) && node.ParentId == relativeParent.Id)).ToArray().
                    OrderBy(node => node.Index).
                    Select((node, idx) => BuildUpTreeNodes(node, allItems, idx, level + 1)
                    ).ToArray();

            if (treeNode.Nodes.Length > 0)
            {
                treeNode.Nodes[0].IsFirst = true;
                treeNode.Nodes[treeNode.Nodes.Length - 1].IsLast = true;
            }

            return treeNode;
        }

        protected virtual NodeQuery GetQuery()
        {
            if (ShowPagesOnly)
            {
                var query = new NodeQuery();
                query.Add(new TypeExpression(ActiveSchema.NodeTypes["Page"]));
                query.Orders.Add(new SearchOrder(IntAttribute.Index));
                return query;
            }

            return null;
        }

        private static string ParentPath(string path)
        {
            return path.Substring(0, path.LastIndexOf('/'));
        }

        private static bool IsSiblingPath(string path1, string path2)
        {
            return ParentPath(path1).Equals(ParentPath(path2));
        }

        private static IEnumerable<string> GetPathCollection(string path)
        {
            var pathItems = path.Split('/');
            for (var i = 1; i < pathItems.Length; i++)
            {
                yield return String.Join("/", pathItems, 0, i + 1);
            }
        }

        protected virtual bool NodeIsValid(Node node)
        {
            if (node == null || node.Name == "(apps)") 
                return false;
            if (TypeNames != null && !TypeNames.Contains(node.NodeType.Name))
                return false;
            var contentNode = node as GenericContent;

            try
            {
                if (!ShowHiddenPages && ((contentNode != null) && contentNode.Hidden))
                    return false;
            }
            catch (InvalidOperationException)
            {
                //"Invalid property access attempt"
                //The user has only See permission for this node. Changing to Admin account does not 
                //help either, because the node is 'head only' and accessing any of its properties 
                //will throw an invalidoperation exception anyway.
                return false;
            }

            if (ExpandToContext)
            {
                var pathCollection = GetPathCollection(PortalContext.Current.Page.Path).ToList().
                    Union(GetPathCollection(ContextNode.Path)).
                    Union(GetPathCollection(PortalContext.Current.ContextNodePath)).
                    ToArray();

                foreach (var path in pathCollection)
                {
                    if (node.Path.Equals(path) || IsSiblingPath(path, node.Path))
                    {
                        return true;
                    }
                }

                if (GetContextChildren)
                {
                    var parentPath = RepositoryPath.GetParentPath(node.Path);
                    if (parentPath.Equals(PortalContext.Current.ContextNode.Path) ||
                        parentPath.Equals(ContextNode.Path))
                        return true;
                }

                return false;
            }

            return true;
        }
    }
}
