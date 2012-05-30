using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using System.Xml.Xsl;
using SenseNet.Search;
using SenseNet.Search.Parser;
using Content = SenseNet.ContentRepository.Content;
using SenseNet.Diagnostics;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Web;
using System.Xml.XPath;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public enum CollectionAxisMode
    {
        Children = 0,
        ReferenceProperty = 1,
        VersionHistory = 2,
        External = 4
    }

    public class ContentCollectionPortlet : ContextBoundPortlet
    {
        public string PortletHash
        {
            get
            {
                return Math.Abs((PortalContext.Current.ContextNode.Path + ID).GetHashCode()).ToString();
            }
        }

        public const string ContentListID = "ContentList";
        // Properties /////////////////////////////////////////////////////////
        #region Properties

        [WebDisplayName("Custom portlet URL key"), WebDescription("Give a cutom key to refer to this portlet from the URL.")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order), WebOrder(80)]
        public string CustomPortletKey { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Collection source")]
        [WebDescription("The source for the listed collection. 'Children' lists child contents of target, 'ReferenceProperty' lists referenced contents using given Reference property, 'VersionHistory' lists target's versions and 'External' uses ids from url parameters as source.")]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(10)]
        public CollectionAxisMode CollectionAxis { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Reference property name")]
        [WebDescription("The property name to use as the reference source")]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(20)]
        public string ReferenceAxisName { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Uri parameter name")]
        [WebDescription("The Uri parameter name to use as external source")]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(30)]
        public string UriParameterName { get; set; }

        [WebDisplayName("All children")]
        [WebDescription("Leave unchecked if you want to get only direct children of content. Check it if you want to get all children of content.")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory("Collection", EditorCategory.Collection_Order), WebOrder(40)]
        public bool AllChildren { get; set; }

        [WebDisplayName("Children filter")]
        [WebDescription("Optional filter for the children query")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(50)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MiddleSize)]
        public string QueryFilter { get; set; }

        [WebDisplayName("Sort by"), WebDescription("Sort field name")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(60)]
        public string SortBy { get; set; }

        [WebDisplayName("Sort descending"), WebDescription("Sort results by sort field ascending or descending")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(70)]
        public bool SortDescending { get; set; }

        [WebDisplayName("Enable paging"), WebDescription("Set it to false to list all contents on one page. Set it to true and use the 'Top' parameter to show only a given number of contents simultaneously")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(80)]
        public bool PagingEnabled { get; set; }

        [WebDisplayName("Show pager control"), WebDescription("Set to true to show default pager controls at the top and the bottom of results")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(90)]
        public bool ShowPagerControl { get; set; }

        [WebDisplayName("Top"), WebDescription("The first given number of contents are listed")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(100)]
        public int Top { get; set; }

        [WebDisplayName("Skip first")]
        [WebDescription("The first given number of contents are skipped")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(110)]
        public int SkipFirst { get; set; }

        [WebDisplayName("Enable autofilters")]
        [WebDescription("If autofilters are enabled, system contents are not shown in the result")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(120)]
        public FilterStatus EnableAutofilters { get; set; }

        [WebDisplayName("Enable lifespan filter")]
        [WebDescription("If lifespan filter is enabled, only contents with valid StartDate and EndDate will be in the result")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(130)]
        public FilterStatus EnableLifespanFilter { get; set; }

        [WebDisplayName("Visible fields")]
        [WebDescription("A comma separated list of fields presented in the list")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(140)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MiddleSize)]
        public string VisibleFields { get; set; }
        #endregion


        public string[] VisisbleFieldNames
        {
            get
            {
                if (string.IsNullOrEmpty(VisibleFields))
                    return new string[] { };
                var vals = VisibleFields.Split(new[] { ',' });
                var result = vals.Select(s => s.Trim()).ToArray();
                return result;
            }
        }


        private List<Node> requestNodeList;
        public List<Node> RequestNodeList
        {
            get
            {
                return requestNodeList ??
                       (requestNodeList = RequestIdList.Count > 0 ? Node.LoadNodes(RequestIdList) : new List<Node>());
            }
        }

        private List<int> requestIdList;
        public List<int> RequestIdList
        {
            get
            {
                // collection's nodeid list comes from url
                if (requestIdList == null)
                {
                    var idList = new List<int>();
                    if (String.IsNullOrEmpty(UriParameterName))
                        return requestIdList = idList;

                    try
                    {
                        var ids = Page.Request[UriParameterName];
                        if (string.IsNullOrEmpty(ids))
                            return requestIdList = idList;

                        foreach (var idString in ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            int nodeId;
                            if (int.TryParse(idString, out nodeId) && nodeId != 0)
                                idList.Add(nodeId);
                        }
                    }
                    catch (Exception ex)
                    {
                        //likely a request error
                        Logger.WriteException(ex);
                    }

                    requestIdList = idList.Distinct().ToList();
                }

                return requestIdList;
            }
        }

        // Constructors ///////////////////////////////////////////////////
        public ContentCollectionPortlet()
        {
            Name = "Content collection";
            Description = "An all-purpose portlet for interacting with collections (context bound)";
            Category = new PortletCategory(PortletCategoryType.Collection);

            Cacheable = true;       // by default, any contentcollection portlet is cached (for visitors)
            CacheByParams = true;   // by default, params are also included -> this is useful for collections with paging
        }

        internal string GetPortletSpecificParamName(string paramName)
        {
            if (string.IsNullOrEmpty(this.CustomPortletKey))
                return PortletHash + "@" + paramName;
            else
                return this.CustomPortletKey + "@" + paramName;
        }

        public string GetPropertyActionUrlPart(string paramName, string paramValue)
        {
            var result = GetPortletSpecificParamName(paramName) + "=" + paramValue;
            return result;
        }

        private bool GetIntFromRequest(string paramName, out int value)
        {
            value = 0;

            if (!Page.Request.Params.AllKeys.Contains(paramName))
                return false;
            var svalue = Page.Request.Params[paramName];
            return !string.IsNullOrEmpty(svalue) && int.TryParse(svalue, out value);
        }
        // Methods /////////////////////////////////////////////////////////

        protected override void PrepareXsltRendering(object model)
        {
            var c = model as Content;
            if (c != null)
                c.AllChildren = AllChildren;
        }
        protected override object SerializeModel(object model)
        {
            var c = model as Content;
            var fc = model as FeedContent;
            switch (CollectionAxis)
            {
                case CollectionAxisMode.Children:
                case CollectionAxisMode.VersionHistory:
                    if (c != null)
                        return c.GetXml(c.ChildrenQueryFilter, c.ChildrenQuerySettings);
                    else
                        return fc.GetXml();
                case CollectionAxisMode.ReferenceProperty:
                    if (c != null)
                        return c.GetXml(ReferenceAxisName);
                    else
                        return fc.GetXml();
                case CollectionAxisMode.External:
                    if (c != null)
                        return c.GetXml(true);
                    else
                        return fc.GetXml(true);
            }
            return null;
        }

        protected override void RenderWithAscx(HtmlTextWriter writer)
        {
            base.RenderContents(writer);
        }

        ContentCollectionPortletState _state;

        public virtual ContentCollectionPortletState State
        {

            get
            {
                if (_state == null)
                {
                    PortletState state;
                    if (PortletState.Restore(this, out state))
                    {
                        _state = state as ContentCollectionPortletState;
                    }
                    else
                    {
                        _state = new ContentCollectionPortletState(this) {Portlet = this};
                    }
                    _state.CollectValues();
                    HttpContext.Current.Session[_state.Portlet.ID] = _state;

                }
                return _state;
            }
        }

        protected override object GetModel()
        {
            if (ContextNode == null)
                return null;

            var content = Content.Create(ContextNode);

            var qs = new QuerySettings();

            if (EnableAutofilters != FilterStatus.Default)
                qs.EnableAutofilters = (EnableAutofilters == FilterStatus.Enabled);
            if (EnableLifespanFilter != FilterStatus.Default)
                qs.EnableLifespanFilter = (EnableLifespanFilter == FilterStatus.Enabled);

            if (Top > 0)
                qs.Top = Top;
            if (State.Skip > 0)
                qs.Skip = State.Skip;

            if (!string.IsNullOrEmpty(State.SortColumn))
            {
                qs.Sort = new[] { new SortInfo { FieldName = State.SortColumn, Reverse = State.SortDescending } };
            }

            content.ChildrenQuerySettings = qs;
            content.ChildrenQueryFilter = GetQueryFilter();

            content.XmlWriterExtender = writer => { };

            switch (CollectionAxis)
            {
                case CollectionAxisMode.Children:
                    return content;
                case CollectionAxisMode.VersionHistory:
                    var versionRoot = SearchFolder.Create(content.Versions);

                    return versionRoot;
                case CollectionAxisMode.ReferenceProperty:
                    return content;
                case CollectionAxisMode.External:
                    return SearchFolder.Create(RequestNodeList);
            }

            return null;
        }

        protected virtual string GetQueryFilter()
        {
            return ReplaceTemplates(QueryFilter);
        }

        protected XPathNavigator XmlModelData { get; set; }

        protected override XsltArgumentList GetXsltArgumentList()
        {
            var arglist = base.GetXsltArgumentList();
            if (XmlModelData != null)
            {
                arglist.AddParam("Model", string.Empty, XmlModelData.Select("/Model"));
            }
            return arglist;
        }

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            Content modelData;
            try
            {
                modelData = GetModel() as Content;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
                return;
            }

            var rootContent = modelData as Content;
            if (rootContent != null) rootContent.AllChildren = AllChildren;

            var model = new ContentCollectionViewModel { State = this.State };

            try
            {
                var childCount = rootContent != null ? rootContent.ChildCount : 0;
                model.Pager = GetPagerModel(childCount, State, string.Empty);
                model.ReferenceAxisName = CollectionAxis == CollectionAxisMode.ReferenceProperty ? ReferenceAxisName : null;

                if (RenderingMode == RenderMode.Xslt)
                {
                    try
                    {
                        XmlModelData = model.ToXPathNavigator();
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(ex.ToString());
                        Logger.WriteException(ex);
                    }
                }
                else if (RenderingMode == RenderMode.Ascx || RenderingMode == RenderMode.Native)
                {
                    model.Content = rootContent;

                    var viewPath = RenderingMode == RenderMode.Native
                                       ? "/root/Global/Renderers/ContentCollectionView.ascx"
                                       : Renderer;
                    Controls.Clear();

                    try
                    {
                        var presenter = Page.LoadControl(viewPath);
                        if (presenter is ContentCollectionView)
                        {
                            ((ContentCollectionView)presenter).Model = model;
                        }
                        if (rootContent != null)
                        {
                            var itemlist = presenter.FindControl(ContentListID);
                            if (itemlist != null)
                            {
                                ContentQueryPresenterPortlet.DataBindingHelper.SetDataSourceAndBind(itemlist,
                                                                                                    rootContent.Children);
                            }
                        }

                        var itemPager = presenter.FindControl("ContentListPager");
                        if (itemPager != null)
                        {
                            ContentQueryPresenterPortlet.DataBindingHelper.SetDataSourceAndBind(itemPager,
                                                                                                model.Pager.PagerActions);
                        }

                        Controls.Add(presenter);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);

                        Controls.Clear();
                        Controls.Add(new LiteralControl("Please select a content view"));
                    }
                }
            }
            catch (ParserException ex)
            {
                var errorText = new LiteralControl { Text = ex.Message };
                Controls.Add(errorText);
            }
            catch (Exception ex)
            {
                var errorText = new LiteralControl { Text = ex.ToString() };
                Controls.Add(errorText);
            }
            ChildControlsCreated = true;
        }

        protected virtual PagerModel GetPagerModel(int totalCount, ContentCollectionPortletState state, string pageUrl)
        {
            return new PagerModel(totalCount, state, pageUrl);
        }
    }
}
