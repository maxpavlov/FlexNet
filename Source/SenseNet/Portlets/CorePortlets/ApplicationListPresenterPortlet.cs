using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ApplicationModel;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public class ApplicationListPresenterPortlet : ContextBoundPortlet
    {
        public ApplicationListPresenterPortlet()
        {
            Name = "Application list presenter";
            Description = "This portlet shows the relevant applications for a content (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Content);

            this.HiddenProperties.Add("Renderer");
        }

        private string _controlPath = "/Root/System/SystemPlugins/Portlets/ApplicationList/ApplicationList.ascx";
        private bool _isHidden = true;

        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ControlPath
        {
            get { return _controlPath; }
            set { _controlPath = value; }
        }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Hidden")]
        [WebDescription("The initial state of the application list")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        public bool IsHidden
        {
            get { return _isHidden; }
            set { _isHidden = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        private ListView _appListView;
        protected ListView ApplicationListView
        {
            get
            {
                return _appListView ?? (_appListView = this.FindControlRecursive("ApplicationListView") as ListView);
            }
        }

        //================================================================ Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();

            try
            {
                var viewControl = Page.LoadControl(ControlPath) as UserControl;
                if (viewControl != null)
                {
                    Controls.Add(viewControl);
                    FillControls();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            ChildControlsCreated = true;
        }

        //================================================================ Helper methods

        protected void FillControls()
        {
            if (ApplicationListView == null)
                return;
            var apps = ApplicationStorage.Instance.GetApplications(ContentRepository.Content.Create(ContextNode), PortalContext.Current.DeviceName);

            //var filteredList = apps.Where(a => a.ScenarioList.Contains("ListItem") || a.Path.Contains("/(apps)/This/")).ToArray();

            //ApplicationListView.DataSource = filteredList;
            ApplicationListView.DataSource = apps;
            ApplicationListView.DataBind();
        }
    }
}
