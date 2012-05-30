using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using Content = SenseNet.ContentRepository.Content;
using System.ComponentModel;

namespace SenseNet.Portal.Portlets
{
    public class ContentHistoryPortlet : ContextBoundPortlet
    {
        public ContentHistoryPortlet()
        {
            this.Name = "Version history";
            this.Description = "This portlet shows the content version history (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        //================================================================ Properties

        private static readonly string DEFAULT_VIEW_PATH = "/Root/System/SystemPlugins/Portlets/ContentHistory/History.ascx";

        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the UI elements of the history portlet")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPath { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        private Label _contentLabel;
        protected Label ContentLabel
        {
            get { return _contentLabel ?? (_contentLabel = this.FindControlRecursive("ContentName") as Label); }
        }

        private ListView _historyListView;
        protected ListView HistoryListView
        {
            get { return _historyListView ?? (_historyListView = this.FindControlRecursive("HistoryListView") as ListView); }
        }

        //================================================================ Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();

            try
            {
                var viewControl = Page.LoadControl(string.IsNullOrEmpty(ViewPath) ? DEFAULT_VIEW_PATH : ViewPath) as UserControl;
                if (viewControl != null)
                {
                    Controls.Add(viewControl);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            if (HistoryListView != null)
                HistoryListView.ItemDataBound += HistoryListView_ItemDataBound;

            var genericContent = GetContextNode() as GenericContent;
            if (genericContent == null)
            {
                //ShowError("This type of content doesn't have a version history");
                return;
            }

            if (ContentLabel != null)
                ContentLabel.Text = genericContent.DisplayName;

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers

        protected void HistoryListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
                return;

            var content = dataItem.DataItem as Content;
            if (content == null)
                return;

            if (!content.IsLatestVersion) 
                return;

            //hide restore button if this is the lates version
            var alb = dataItem.FindControlRecursive("RestoreButton") as ActionLinkButton;
            if (alb != null)
                alb.Visible = false;
        }
    }
}
