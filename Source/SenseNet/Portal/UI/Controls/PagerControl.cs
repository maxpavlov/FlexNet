using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.UI.Controls
{
    public class PagerControl : UserControl
    {
        #region Member variables

        public event EventHandler OnPageSelected;
        public event EventHandler OnPageSizeChanged;

        protected Repeater PageRepeater;
        protected ListControl PageSizeListControl;
        protected PlaceHolder PageSizePanel;

        private int _pageSize;
        private int _resultCount;
        private int _currentPage;
        private int _visiblePageCount = 10;
        private int _morePagesSkipCount = 10;
        private bool _showUnusedLinks;
        private bool _enableSettingPageSize = true;
        private List<int> _pageSizeList;

        private const string CssDisabledPostfix = "_Disabled";

        #endregion

        //-- Properties ----------------------------------------------------------------------

        private bool Changed { get; set; }

        public int CurrentPage
        {
            get { return Math.Max(1, _currentPage); }
            set
            {
                Changed = _currentPage != value;
                _currentPage = value;
            }
        }

        public int VisiblePageCount
        {
            get { return Math.Max(1, _visiblePageCount); }
            set
            {
                Changed = _visiblePageCount != value;
                _visiblePageCount = value;
            }
        }

        public int MorePagesSkipCount
        {
            get { return Math.Max(1, _morePagesSkipCount); }
            set
            {
                Changed = _morePagesSkipCount != value;
                _morePagesSkipCount = value;
            }
        }

        public bool ShowUnusedLinks
        {
            get { return _showUnusedLinks; }
            set
            {
                Changed = _showUnusedLinks != value;
                _showUnusedLinks = value;
            }
        }

        private bool ShowInvisiblePanelBefore { get; set; }
        private bool ShowInvisiblePanelAfter { get; set; }

        /// <summary>
        /// Controls the visibility of the page size chooser dropdown list or radiobutton list
        /// </summary>
        public bool EnableSettingPageSize
        {
            get { return _enableSettingPageSize; }
            set
            {
                Changed = _enableSettingPageSize != value;
                _enableSettingPageSize = value;
            }
        }

        /// <summary>
        /// Items to display in the page size chooser dropdown list or radiobutton list
        /// </summary>
        public List<int> PageSizeList
        {
            get
            {
                if (_pageSizeList == null)
                {
                    if (PageSizeListControl != null && PageSizeListControl.Items.Count > 0)
                    {
                        _pageSizeList = new List<int>();

                        try
                        {
                            foreach (ListItem li in PageSizeListControl.Items)
                            {
                                _pageSizeList.Add(Convert.ToInt32(li.Value));
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteException(ex);
                        }

                    }
                    else
                    {
                        _pageSizeList = new List<int> { 10, 50 };
                    }
                }

                return _pageSizeList;
            }
            set
            {
                Changed = true;
                _pageSizeList = value;
            }
        }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize
        {
            get { return _pageSize < 1 ? 10 : _pageSize; }
            set
            {
                Changed = _pageSize != value;
                _pageSize = value;
            }
        }

        /// <summary>
        /// Total item count
        /// </summary>
        public int ResultCount
        {
            get { return _resultCount; }
            set
            {
                Changed = _resultCount != value;
                _resultCount = value;
            }
        }

        /// <summary>
        /// Number of pages, calculated value
        /// </summary>
        public int PageCount
        {
            get
            {
                if (ResultCount == 0 || PageSize == 0)
                    return 0;

                var pageCount = ResultCount / PageSize;
                if (pageCount * PageSize < ResultCount)
                    pageCount++;

                return pageCount;
            }
        }

        //-- Control state ------

        protected override void OnInit(EventArgs e)
        {
            this.Page.RegisterRequiresControlState(this);
            base.OnInit(e);
        }

        protected override object SaveControlState()
        {
            var state = new object[4];

            state[0] = base.SaveControlState();
            state[1] = this.CurrentPage;
            state[2] = this.PageSize;
            state[3] = this.ResultCount;

            return state;
        }

        protected override void LoadControlState(object savedState)
        {
            var state = savedState as object[];

            if (state == null || state.Length != 4)
            {
                base.LoadControlState(savedState);
                return;
            }

            base.LoadControlState(state[0]);

            if (state[1] != null)
                this.CurrentPage = (int)state[1];
            if (state[2] != null)
                this.PageSize = (int)state[2];
            if (state[3] != null)
                this.ResultCount = (int)state[3];
        }

        //-- Methods -------------------------------------------------------------------------

        /// <summary>
        /// Refresh the whole html by re-calculating the visible 
        /// page numbers and databinding the main repeater
        /// </summary>
        private void RebuildControls()
        {
            Changed = false;

            if (PageSizePanel != null)
            {
                //get PageSizeList property before clearing the list control, 
                //because first time the source is the control itself!
                if (this.EnableSettingPageSize && PageSizeListControl != null && PageSizeList.Count > 0)
                {
                    PageSizeListControl.Items.Clear();
                    var selected = 0;

                    foreach (var size in PageSizeList)
                    {
                        PageSizeListControl.Items.Add(new ListItem(size.ToString(), size.ToString()));

                        if (size == this.PageSize)
                            selected = PageSizeListControl.Items.Count - 1;
                    }

                    if (PageSizeListControl.Items.Count > selected)
                        PageSizeListControl.SelectedIndex = selected;
                }
                else
                {
                    PageSizePanel.Visible = false;
                }
            }

            if (PageRepeater == null)
                return;

            if (PageCount == 0)
            {
                PageRepeater.Visible = false;

                if (PageSizePanel != null)
                    PageSizePanel.Visible = false;

                return;
            }

            var pageList = new List<int>();
            var pageCount = PageCount;

            if (pageCount <= VisiblePageCount)
            {
                for (var i = 1; i <= pageCount; i++)
                    pageList.Add(i);
            }
            else
            {
                var first = Math.Max(1, CurrentPage - VisiblePageCount / 2);
                var last = first + VisiblePageCount - 1;

                if (last > pageCount)
                {
                    last = pageCount;
                    first = last - VisiblePageCount + 1;
                }

                for (var i = first; i <= last; i++)
                    pageList.Add(i);

                ShowInvisiblePanelBefore = first > 1;
                ShowInvisiblePanelAfter = last < pageCount;
            }

            if (pageList.Count > 1)
            {
                PageRepeater.DataSource = pageList;
                PageRepeater.DataBind();
                PageRepeater.Visible = true;
            }
            else
            {
                PageRepeater.DataSource = null;
                PageRepeater.DataBind();
                PageRepeater.Visible = false;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            RebuildControls();
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (Changed)
                RebuildControls();

            base.OnPreRender(e);
        }

        private void ReplaceControl(WebControl controlToHide, Control controlToShow)
        {
            ReplaceControl(controlToHide, controlToShow, false);
        }

        /// <summary>
        /// Hides the first control and shows the second. If there is no second, 
        /// the first will be only disabled and gets a css postfix
        /// </summary>
        private void ReplaceControl(WebControl controlToHide, Control controlToShow, bool forceVisible)
        {
            if (ShowUnusedLinks || forceVisible)
            {
                if (controlToShow != null)
                {
                    controlToShow.Visible = true;

                    if (controlToHide != null)
                        controlToHide.Visible = false;
                }
                else
                {
                    if (controlToHide != null)
                    {
                        controlToHide.Enabled = false;
                        controlToHide.CssClass += CssDisabledPostfix;
                    }
                }
            }
            else
            {
                if (controlToHide != null)
                    controlToHide.Visible = false;
            }
        }

        //-- Event handlers ------------------------------------------------------------------

        protected void PageSizeListControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.PageSize = Convert.ToInt32(PageSizeListControl.SelectedItem.Value);
                this.CurrentPage = 1;

                if (OnPageSizeChanged != null)
                    OnPageSizeChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        /// <summary>
        /// This method controls the visibility of the navigation controls
        /// (selected page, back and forward links)
        /// </summary>
        protected void Repeater_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                switch (e.Item.ItemType)
                {
                    case ListItemType.AlternatingItem:
                    case ListItemType.Item:
                        if (Convert.ToInt32(e.Item.DataItem) == CurrentPage)
                        {
                            var lnkPage = e.Item.FindControl("lnkPage") as WebControl;
                            var lnkPageDisabled = e.Item.FindControl("lnkPageDisabled") as WebControl;

                            ReplaceControl(lnkPage, lnkPageDisabled, true);
                        }
                        break;
                    case ListItemType.Header:
                        if (CurrentPage <= 1)
                        {
                            var blink = e.Item.FindControl("lnkBack") as WebControl;
                            var blinkMore = e.Item.FindControl("lnkBackMore") as WebControl;
                            var linkFirst = e.Item.FindControl("lnkFirst") as WebControl;
                            var blinkDisabled = e.Item.FindControl("lnkBackDisabled") as WebControl;
                            var blinkMoreDisabled = e.Item.FindControl("lnkBackMoreDisabled") as WebControl;
                            var linkFirstDisabled = e.Item.FindControl("lnkFirstDisabled") as WebControl;

                            ReplaceControl(blink, blinkDisabled);
                            ReplaceControl(blinkMore, blinkMoreDisabled);
                            ReplaceControl(linkFirst, linkFirstDisabled);
                        }

                        var invPagesBefore = e.Item.FindControl("InvisiblePagesBefore");
                        if (invPagesBefore != null)
                            invPagesBefore.Visible = ShowInvisiblePanelBefore;
                        break;
                    case ListItemType.Footer:
                        if (CurrentPage >= PageCount)
                        {
                            var blink = e.Item.FindControl("lnkForward") as WebControl;
                            var blinkMore = e.Item.FindControl("lnkForwardMore") as WebControl;
                            var linkLast = e.Item.FindControl("lnkLast") as WebControl;
                            var blinkDisabled = e.Item.FindControl("lnkForwardDisabled") as WebControl;
                            var blinkMoreDisabled = e.Item.FindControl("lnkForwardMoreDisabled") as WebControl;
                            var linkLastDisabled = e.Item.FindControl("lnkLastDisabled") as WebControl;

                            ReplaceControl(blink, blinkDisabled);
                            ReplaceControl(blinkMore, blinkMoreDisabled);
                            ReplaceControl(linkLast, linkLastDisabled);
                        }

                        var invPagesAfter = e.Item.FindControl("InvisiblePagesAfter");
                        if (invPagesAfter != null)
                            invPagesAfter.Visible = ShowInvisiblePanelAfter;
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        /// <summary>
        /// This method controls the actually selected page number after
        /// the user pressed one of the navigation links
        /// </summary>
        protected void Repeater_OnItemCommand(object sender, RepeaterCommandEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Separator || e.Item.ItemType == ListItemType.EditItem)
                return;

            switch (e.CommandName)
            {
                case "PageFirst":
                    CurrentPage = 1;
                    break;
                case "PageBack":
                    CurrentPage = Math.Max(1, CurrentPage - 1);
                    break;
                case "PageBackMore":
                    CurrentPage = Math.Max(1, CurrentPage - MorePagesSkipCount);
                    break;
                case "PageForward":
                    CurrentPage = Math.Min(CurrentPage + 1, PageCount);
                    break;
                case "PageForwardMore":
                    CurrentPage = Math.Min(CurrentPage + MorePagesSkipCount, PageCount);
                    break;
                case "PageLast":
                    CurrentPage = PageCount;
                    break;
                case "PageSelected":
                    CurrentPage = Convert.ToInt32(e.CommandArgument);
                    break;
            }

            if (OnPageSelected != null)
                OnPageSelected(this, EventArgs.Empty);
        }
    }
}
