using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.Portlets
{
    public class ActionListPresenterPortlet : ContextBoundPortlet
    {
        public ActionListPresenterPortlet()
        {
            Name = "Action list presenter";
            Description = "This portlet shows an ActionMenu (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Portal);
        }

        private string _controlPath = "/Root/System/SystemPlugins/Controls/ActionListPresenter.ascx";

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ControlPath
        {
            get { return _controlPath; }
            set { _controlPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Scenario name")]
        [WebDescription("Name of the Scenario")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        public string Scenario { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Scenario parameters")]
        [WebDescription("Additional advanced scenario parameters (ie. Portlet ID when service actions require it)")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(120)]
        public string ScenarioParameters { get; set; }

        //FIXME - unused property
        [WebBrowsable(false), Personalizable(true)]
        [WebDisplayName("ActionMenu mode")]
        [WebDescription("Defines action menu layout")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(130)]
        public ActionMenuMode Mode { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Text")]
        [WebDescription("Displayed text of the action list")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(140)]
        public string ActionListText { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Icon path")]
        [WebDescription("The full path of the icon of the action menu")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Icon)]
        [WebOrder(150)]
        public string IconUrl { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Wrapper css class")]
        [WebDescription("Container div css class for the ActionMenu")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(160)]
        public string WrapperCssClass { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Item hover css class")]
        [WebDescription("Item hover css class for the ActionMenu items")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(170)]
        public string ItemHoverCssClass { get; set; }

        //default value is true
        private bool _actionIconVisible = true;

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Action icons are visible")]
        [WebDescription("Controls the visibility of the action icons")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(180)]
        public bool ActionIconVisible
        {
            get { return _actionIconVisible; }
            set { _actionIconVisible = value; }
        }

        private ActionMenu _actionMenu;
        protected ActionMenu ActionMenu
        {
            get { return _actionMenu ?? (_actionMenu = this.FindControlRecursive("ActionMenu") as ActionMenu); }
        }

        private ActionList _actionList;
        protected ActionList ActionList
        {
            get { return _actionList ?? (_actionList = this.FindControlRecursive("ActionList") as ActionList); }
        }

        private ListView _actionListView;
        protected ListView ActionListView
        {
            get { return _actionListView ?? (_actionListView = this.FindControlRecursive("ActionListView") as ListView); }
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
                    SetParameters();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            ChildControlsCreated = true;
        }

        //================================================================ Helper methods

        private void SetParameters()
        {
            if (ActionMenu != null)
            {
                ActionMenu.Scenario = Scenario;
                ActionMenu.Text = ActionListText;
                ActionMenu.IconUrl = IconUrl;
                ActionMenu.ScenarioParameters = ScenarioParameters;
                ActionMenu.WrapperCssClass = WrapperCssClass;
                ActionMenu.ItemHoverCssClass = ItemHoverCssClass;

                if (ContextNode != null)
                    ActionMenu.NodePath = ContextNode.Path;
            }

            if (ActionList != null)
            {
                ActionList.Scenario = Scenario;
                ActionList.Text = ActionListText;
                ActionList.ScenarioParameters = ScenarioParameters;
                ActionList.WrapperCssClass = WrapperCssClass;
                ActionList.ActionIconVisible = ActionIconVisible;

                if (ContextNode != null)
                    ActionList.NodePath = ContextNode.Path;
            }
        }
    }
}
