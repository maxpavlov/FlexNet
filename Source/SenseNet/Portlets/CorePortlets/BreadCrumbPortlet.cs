using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using SNP = SenseNet.Portal;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Portal.Portlets
{
    public class BreadCrumbPortlet : ContextBoundPortlet
    {
        // Constructor ////////////////////////////////////////////////////////////

        public BreadCrumbPortlet()
        {
            this.Name = "Breadcrumb trail";
            this.Description = "Helps to keep track of location relative to the site root (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Navigation);
        }

        // Members and properties /////////////////////////////////////////////////
        private string _separator = " / ";
        private string _currentSiteUrl;
        private string _linkCssClass = string.Empty;
        private string _itemCssClass = string.Empty;
        private string _separatorCssClass = string.Empty;
        private string _activeItemCssClass = string.Empty;
        private bool _showSite = false;
        private string _siteDisplayName = string.Empty;
        private List<Node> _pathNodeList;
        private int _actualNodeLevel = 0;

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("CSS class name for items")]
        [WebDescription("This is the css class name for the items in list mode")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        public string ItemCssClass
        {
            get { return _itemCssClass; }
            set { _itemCssClass = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("CSS class name for links")]
        [WebDescription("This is the css class name for the links")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        public string LinkCssClass
        {
            get { return _linkCssClass; }
            set { _linkCssClass = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("CSS class name for separator")]
        [WebDescription("This is the css class name for the separator string")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(120)]
        public string SeparatorCssClass
        {
            get { return _separatorCssClass; }
            set { _separatorCssClass = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Breadcrumb separator")]
        [WebDescription("This is the separator string between elements in linear mode")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(130)]
        public string Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Active item CSS class name")]
        [WebDescription("This is the css class name for the active item")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(140)]
        public string ActiveItemCssClass
        {
            get { return _activeItemCssClass; }
            set { _activeItemCssClass = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Show site name")]
        [WebDescription("Show the site name in the beginning of the breadcrumb or not")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(150)]
        public bool ShowSite
        {
            get { return _showSite; }
            set { _showSite = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Site display name")]
        [WebDescription("This will be displayed as the name of the site")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(160)]
        public string SiteDisplayName
        {
            get { return _siteDisplayName; }
            set { _siteDisplayName = value; }
        }

        public string CurrentSiteUrl
        {
            get
            {
                if (String.IsNullOrEmpty(_currentSiteUrl))
                {
                    foreach (string urlItem in PortalContext.Current.Site.UrlList.Keys)
                    {
                        string requestUrl = string.Concat(HttpContext.Current.Request.Url.ToString(), "/");
                        if (requestUrl.IndexOf(string.Concat(urlItem, "/")) != -1)
                        {
                            _currentSiteUrl = urlItem;
                            break;
                        }
                    }
                }
                return _currentSiteUrl;
            }
        }
        private bool HasAppModel
        {
            get
            {
                var n = PortalContext.Current.ContextNode;
                return n != null;
            }
        }

        // Events /////////////////////////////////////////////////////////////////
        protected override void RenderContents(HtmlTextWriter writer)
        {
            RenderContentsInternal(writer);
            base.RenderContents(writer);
        }

        // Internals //////////////////////////////////////////////////////////////
        private void RenderContentsInternal(HtmlTextWriter writer)
        {
            Node actualNode;

            if (HasAppModel)
                actualNode = PortalContext.Current.ContextNode;
            else
            {
                switch (BindTarget)
                {
                    //case BindTarget.CurrentSite:
                    //case BindTarget.CurrentPage:
                    case BindTarget.CurrentContent:
                    case BindTarget.CurrentStartPage:
                    case BindTarget.CustomRoot:
                        actualNode = GetContextNode();
                        break;
                    default:
                        actualNode = SenseNet.Portal.Page.Current;
                        break;
                }

            }

            _pathNodeList = new List<Node>();

            if (actualNode != null)
            {
                _pathNodeList.Add(actualNode);
                SetActualParent(actualNode, 0);
                RenderBreadCrumbItems(writer, _actualNodeLevel);
            }
            else if (this.RenderException != null)
            {
                writer.Write(String.Concat(String.Concat("Portlet Error: ", this.RenderException.Message), this.RenderException.InnerException == null ? string.Empty : this.RenderException.InnerException.Message));
            }
        }
        private void SetActualParent(Node actualNode, int index)
        {
            _actualNodeLevel = index;

            if (actualNode.NodeType.IsInstaceOfOrDerivedFrom("Site"))
                return;

            var parentHead = NodeHead.Get(actualNode.ParentId);
            var parent = parentHead == null
                             ? null
                             : SecurityHandler.HasPermission(parentHead, PermissionType.See, PermissionType.Open)
                                   ? actualNode.Parent
                                   : null;
            if (parent != null)
            {
                index++;
                _pathNodeList.Add(parent);
                SetActualParent(parent, index);
            }
            else return;
        }
        private void RenderBreadCrumbItems(HtmlTextWriter writer, int index)
        {
            for (int i = _actualNodeLevel; i >= 0; i--)
            {
                var currentNode = _pathNodeList[i];
                var actualPage = currentNode as Page;
                if (actualPage == null
                    && !HasAppModel)
                    continue;

                // No Root displayed
                if (currentNode.Path.Equals(Repository.RootPath))
                    continue;

                string displayName;
                string pageHref;

                if (currentNode.Path.Equals(PortalContext.Current.Site.Path))
                {
                    if (ShowSite)
                    {
                        displayName = !string.IsNullOrEmpty(SiteDisplayName) ? SiteDisplayName : ((GenericContent)currentNode).DisplayName;
                        pageHref = "/";
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    displayName = currentNode is GenericContent ? ((GenericContent)currentNode).DisplayName : currentNode.Name;
                    var replacedSitePath = ReplaceSitePath(currentNode.Path);
                    pageHref = ProcessUrl(replacedSitePath);
                }

                var renderLink = i > 0;

                RenderBreadCrumbItems(writer, pageHref, displayName, renderLink);

                if (i == 0)
                    continue;

                writer.Write(string.Format("<span class='{0}'>", SeparatorCssClass));
                writer.WriteEncodedText(Separator);
                writer.Write("</span>");
            }
        }
        private string ReplaceSitePath(string path)
        {
            return path.Replace(PortalContext.Current.Site.Path, CurrentSiteUrl);
        }
        private static string ProcessUrl(string url)
        {
            return url.Contains("/") ? url.Substring(url.IndexOf('/')) : url;
        }

        private void RenderBreadCrumbItems(HtmlTextWriter writer, string href, string menuText, bool renderLink)
        {
            if (renderLink)
                writer.Write(string.Format("<a class=\"{0} {1}\" href=\"{2}\"><span>{3}</span></a>", ItemCssClass,
                                           LinkCssClass, href, menuText));
            else
                writer.Write(string.Format("<span class=\"{0} {1}\"><span>{2}</span></span>", ItemCssClass,
                                           ActiveItemCssClass, menuText));
        }
    }
}