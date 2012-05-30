using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using System.Linq;
using SenseNet.Diagnostics;
using SenseNet.Search;

namespace SenseNet.Portal.Portlets
{
    public class ContentQueryPortlet : CacheablePortlet
    {
        //-- Variables ----------------------------------------------------

        string _queryString = string.Empty;
        string _queryString2 = string.Empty;
        string _queryString3 = string.Empty;
        string _cvPath = string.Empty;
        int _pageSize;
        int _currentPage = 1;

        //-- Properties ---------------------------------------------------

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Query string 1")]
        [WebDescription("Default query defining the set of contents to be presented using Content Query Language")]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(10)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string QueryString
        {
            get { return _queryString; }
            set { _queryString = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Query string 2")]
        [WebDescription("Query defining the set of contents to be presented using Content Query Language. This query is selected with <Url prefix>CQPQueryNumber url parameters")]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(20)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string QueryString2
        {
            get { return _queryString2; }
            set { _queryString2 = value; }
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Query string 3")]
        [WebDescription("Query defining the set of contents to be presented using Content Query Language. This query is selected with <Url prefix>CQPQueryNumber url parameters")]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(30)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string QueryString3
        {
            get { return _queryString3; }
            set { _queryString3 = value; }
        }

        [WebDisplayName("Url prefix")]
        [WebDescription("Url param prefix used to select this portlet instance. This prefix is combined with CQPQueryNumber, CQPStartIndex and CQPPageSize strings")]
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(40)]
        public string UrlParamPreFix { get; set; }

        [WebDisplayName("Run as system")]
        [WebDescription("Execute the query in an elevated security context")]
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(50)]
        public bool IsSystemAccount { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Enable paging")]
        [WebDescription("Set it to false to list all contents on one page. Set it to true to show only a given number of contents simultaneously")]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(60)]
        public bool EnablePaging
        {
            get;
            set;
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Setting page size enabled")]
        [WebDescription("Set it to false to always use the default page size given")]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(70)]
        public bool EnableSettingPageSize
        {
            get;
            set;
        }

        [WebDisplayName("Default page size")]
        [WebDescription("The given number of contents are listed simultaneously when pageing is enabled")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(80)]
        public int DefaultPageSize
        {
            get;
            set;
        }

        [WebDisplayName("Top")]
        [WebDescription("The first given number of contents are listed")]
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(90)]
        public int Top { get; set; }

        [WebDisplayName("Skip first")]
        [WebDescription("The first given number of contents are skipped")]
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(100)]
        public int SkipFirst { get; set; }

        [WebDisplayName("Enable autofilters")]
        [WebDescription("If autofilters are enabled, system contents are not shown in the result")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(180)]
        public FilterStatus EnableAutofilters
        {
            get;
            set;
        }

        [WebDisplayName("Enable lifespan filter")]
        [WebDescription("If lifespan filter is enabled, only contents with valid StartDate and EndDate will be in the result")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order)]
        [WebOrder(190)]
        public FilterStatus EnableLifespanFilter
        {
            get;
            set;
        }

        [WebDisplayName("Query view path")]
        [WebDescription("Path of the .ascx query view (SenseNet.Portal.UI.Controls.QueryView control) to render query results")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string CvPath
        {
            get { return _cvPath; }
            set { _cvPath = value; }
        }

        private int CurrentPage
        {
            get { return Math.Max(1, _currentPage); }
            set { _currentPage = Math.Max(1, value); }
        }

        private int PageSize
        {
            get
            {
                return _pageSize == 0 || !this.EnableSettingPageSize ? this.DefaultPageSize : _pageSize;
            }
            set
            {
                _pageSize = value;
            }
        }

        private string CQPQueryNumber
        {
            get
            {
                var number = HttpContext.Current.Request.Params[string.Concat(UrlParamPreFix, "CQPQueryNumber")];
                return string.IsNullOrEmpty(number) ? "1" : number;
            }
        }

        private int CQPStartIndex
        {
            get
            {
                var startIndex = 0;
                int.TryParse(HttpContext.Current.Request.Params[string.Concat(UrlParamPreFix, "CQPStartIndex")], out startIndex);
                return startIndex;
            }
        }

        private int CQPPageSize
        {
            get
            {
                var pageSize = 0;
                int.TryParse(HttpContext.Current.Request.Params[string.Concat(UrlParamPreFix, "CQPPageSize")], out pageSize);
                return pageSize;
            }
        }

        private bool CQPParamsExist
        {
            get { return this.CQPStartIndex > 0 && this.CQPPageSize > 0; }
        }

        //-- Constructor --------------------------------------------------

        public ContentQueryPortlet()
        {
            this.Name = "Content query";
            this.Description = "Display list of Contents based on flexible queries against the Sense/Net Content Repository (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Search);
        }

        //-- Methods ------------------------------------------------------

        protected override void CreateChildControls()
        {

            if (CanCache && Cacheable && IsInCache)
                return;

            using (var traceOperation = SenseNet.Diagnostics.Logger.TraceOperation("ContentQueryPortlet.CreateChildControls"))
            {
                Controls.Clear();

                try
                {
                    CreateControls();
                }
                catch (Exception ex) //logged
                {
                    Logger.WriteException(ex);
                    Controls.Add(new LiteralControl(string.Concat(ex.Message, "<br />", ex.StackTrace)));
                }

                ChildControlsCreated = true;
                traceOperation.IsSuccessful = true;
            }
        }

        private void CreateControls()
        {
            this.Controls.Clear();

            if (string.IsNullOrEmpty(CvPath))
                return;

            try
            {
                var qv = Page.LoadControl(CvPath) as QueryView;
                if (qv == null)
                    return;

                var contentQuery = GetQuery();
                if (contentQuery == null)
                    return;

                if (IsSystemAccount)
                    AccessProvider.ChangeToSystemAccount();

                //get full result list, without loading the nodes
                var result = contentQuery.Execute();
                var fullCount = result.Count;

                if (EnablePaging)
                {
                    //Get results for current page only.
                    //Merge two mechanisms: url params and paging
                    if (this.CQPParamsExist)
                    {
                        contentQuery.Settings.Skip = (this.CurrentPage - 1) * this.PageSize + this.SkipFirst +
                            this.CQPStartIndex - 1;

                        contentQuery.Settings.Top = this.CQPPageSize;
                    }
                    else
                    {
                        contentQuery.Settings.Skip = (this.CurrentPage - 1) * this.PageSize + this.SkipFirst;
                        contentQuery.Settings.Top = this.PageSize > 0 ? this.PageSize : NodeQuery.UnlimitedPageSize - 1;
                    }

                    result = contentQuery.Execute();
                }

                //refresh pager controls
                foreach (var pc in qv.PagerControls)
                {
                    if (EnablePaging)
                    {
                        pc.ResultCount = fullCount;
                        pc.PageSize = contentQuery.Settings.Top;
                        pc.CurrentPage = this.CurrentPage;
                        pc.EnableSettingPageSize = this.EnableSettingPageSize;
                        pc.OnPageSelected += PagerControl_OnPageSelected;
                        pc.OnPageSizeChanged += PagerControl_OnPageSizeChanged;
                    }
                    else
                    {
                        pc.Visible = false;
                    }
                }

                if (IsSystemAccount)
                    AccessProvider.RestoreOriginalUser();

                qv.ID = "QueryView";
                //qv.NodeItemList = result.CurrentPage.ToList();
                qv.NodeItemList = result.Nodes.ToList();

                this.Controls.Add(qv);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);

                this.Controls.Add(new LiteralControl(ex.ToString()));
            }
        }

        protected void PagerControl_OnPageSizeChanged(object sender, EventArgs e)
        {
            var pager = sender as PagerControl;

            if (pager == null ||
                //this.PageSize == pager.PageSize || 
                !this.EnablePaging)
                return;

            //need to get _all_ of the dinamic properties to
            //refresh portlet property values!
            this.PageSize = pager.PageSize;
            this.CurrentPage = pager.CurrentPage;

            CreateControls();
        }

        protected void PagerControl_OnPageSelected(object sender, EventArgs e)
        {
            var pager = sender as PagerControl;

            if (pager == null ||
                //this.CurrentPage == pager.CurrentPage || 
                !this.EnablePaging)
                return;

            //need to get _all_ of the dinamic properties to
            //refresh portlet property values!
            this.PageSize = pager.PageSize;
            this.CurrentPage = pager.CurrentPage;

            CreateControls();
        }

        private ContentQuery GetQuery()
        {
            var queryString = GetQueryString();

            if (string.IsNullOrEmpty(queryString))
                return null;

            try
            {
                var query = ContentQuery.CreateQuery(queryString);

                if (EnableAutofilters != FilterStatus.Default)
                    query.Settings.EnableAutofilters = (EnableAutofilters == FilterStatus.Enabled);
                if (EnableLifespanFilter != FilterStatus.Default)
                    query.Settings.EnableLifespanFilter = (EnableLifespanFilter == FilterStatus.Enabled);

                if (this.CQPParamsExist)
                {
                    query.Settings.Skip = CQPStartIndex + this.SkipFirst;
                    query.Settings.Top = NodeQuery.UnlimitedPageSize - 1;
                    //CQPPageSize;
                }
                else if (this.SkipFirst > 0)
                {
                    query.Settings.Skip = this.SkipFirst;
                    query.Settings.Top = NodeQuery.UnlimitedPageSize - 1;
                }

                if (Top > 0)
                {
                    query.Settings.Top = Top;
                }

                return query;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                return null;
            }
        }

        private string GetQueryString()
        {
            switch (CQPQueryNumber)
            {
                case "1": return QueryString;
                case "2": return QueryString2;
                case "3": return QueryString3;
            }
            return string.Empty;
        }
    }
}