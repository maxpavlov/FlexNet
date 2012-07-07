using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SNCR=SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:ActionList ID=\"ActionList\" runat=server></{0}:ActionList>")]
    public class ActionList : UserControl, IActionUiAdapter
    {
        private string _controlPath = "/Root/System/SystemPlugins/Controls/ActionList.ascx";

        public string ControlPath
        {
            get { return _controlPath; }
            set { _controlPath = value; }
        }

        //================================================================ IActionUiAdapter

        public string NodePath { get; set; }
        public string Scenario { get; set; }
        public string ScenarioParameters { get; set; }
        public string WrapperCssClass { get; set; }
        public string Text { get; set; }
        public string ContextInfoID { get; set; }
        public string ActionName { get; set; }
        public string ContentPathList { get; set; }

        #region IActionUiAdapter Members

        string IActionUiAdapter.IconName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        string IActionUiAdapter.IconUrl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        bool IActionUiAdapter.OverlayVisible { get; set; }

        #endregion

        //================================================================ Properties

        private bool _actionIconVisible = true;
        public bool ActionIconVisible
        {
            get { return _actionIconVisible; }
            set { _actionIconVisible = value; }
        }

        public bool UseContentIcon { get; set; }

        private ListView _actionListView;
        protected ListView ActionListView
        {
            get { return _actionListView ?? (_actionListView = this.FindControlRecursive("ActionListView") as ListView); }
        }

        private WebControl _wrapperPanel;
        protected WebControl WrapperPanel
        {
            get { return _wrapperPanel ?? (_wrapperPanel = this.FindControlRecursive("ActionListPanel") as WebControl); }
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

        protected override void Render(HtmlTextWriter writer)
        {
            if (WrapperPanel != null && !string.IsNullOrEmpty(WrapperCssClass))
                WrapperPanel.CssClass += " " + WrapperCssClass;

            base.Render(writer);
        }

        //================================================================ Helper methods

        private void SetParameters()
        {
            if (ActionListView == null) 
                return;

            ActionListView.ItemDataBound += ActionListView_ItemDataBound;
            ActionListView.DataSource = null;

            //refresh NodePath by contextinfo id
            if (!string.IsNullOrEmpty(ContextInfoID))
            {
                var context = UITools.FindContextInfo(this, ContextInfoID);
                if (context != null)
                {
                    var path = context.Path;
                    if (!string.IsNullOrEmpty(path))
                        NodePath = path;
                }
            }

            if (!string.IsNullOrEmpty(NodePath))
            {
                var actions = new List<ActionBase>();

                if (string.IsNullOrEmpty(Scenario))
                {
                    actions = ActionFramework.GetActions(ContentRepository.Content.Load(NodePath),
                        Scenario, PortalContext.Current.RequestedUri.PathAndQuery).ToList();
                }
                else
                {
                    var scParams = GetReplacedScenarioParameters();
                    var sc = ScenarioManager.GetScenario(Scenario, scParams);
                    if (sc != null)
                    {
                        actions = sc.GetActions(ContentRepository.Content.Load(NodePath),
                            PortalContext.Current.RequestedUri.PathAndQuery).ToList();
                    }
                }

                ActionListView.DataSource = actions.Count > 0 ? actions : null;
            }
            else if (!string.IsNullOrEmpty(ActionName) && !string.IsNullOrEmpty(ContentPathList))
            {
                var actions = GetActionListFromPathList();

                ActionListView.DataSource = actions.Count > 0 ? actions : null;
            }

            ActionListView.DataBind();
        }

        private List<ActionBase> GetActionListFromPathList()
        {
            var actions = new List<ActionBase>();

            if (string.IsNullOrEmpty(ActionName) || string.IsNullOrEmpty(ContentPathList))
                return actions;

            var pathList = ContentPathList.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);

            actions.AddRange(pathList.Select(SNCR.Content.Load).Select(content => ActionFramework.GetAction(ActionName, content, null)).Where(action => action != null));

            foreach (var action in actions)
            {
                action.Text = action.GetContent().DisplayName;
            }

            return actions;
        }

        //================================================================ Event handlers

        protected void ActionListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
                return;

            var action = dataItem.DataItem as ActionBase;
            if (action == null)
                return;

            var actionLink = dataItem.FindControl("ActionLink") as ActionLinkButton;
            if (actionLink == null)
                return;

            actionLink.Action = action;
            actionLink.Parameters = action.GetParameteres();
            actionLink.ActionName = action.Name;
            actionLink.Text = action.Text;
            actionLink.IconVisible = ActionIconVisible;
            actionLink.NodePath = action.GetContent().Path;

            if (UseContentIcon)
                actionLink.IconName = action.GetContent().Icon;
        }

        //================================================================ Helper methods

        public string GetReplacedScenarioParameters()
        {
            var scParams = ScenarioParameters;
            if (string.IsNullOrEmpty(scParams))
                return scParams;

            //backward compatibility
            if (scParams.StartsWith("{PortletID}"))
                scParams = string.Concat("PortletID=", scParams);

            return TemplateManager.Replace(typeof(PortletTemplateReplacer), scParams, this);
        }
    }
}
