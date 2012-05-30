using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using SenseNet.ApplicationModel;
using Content = SenseNet.ContentRepository.Content;
using System.Web;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:ActionLinkButton ID=\"ActionLinkButton1\" runat=server></{0}:ActionLinkButton>")]
    public class ActionLinkButton : System.Web.UI.WebControls.HyperLink, IActionUiAdapter
    {
        // Properties

        #region IActionUiAdapter Members

        public string NodePath { get; set; }
        public string ContextInfoID { get; set; }
        public string WrapperCssClass { get; set; }
        public string Scenario { get; set; }
        public string ScenarioParameters { get; set; }
        public string ActionName { get; set; }

        private string _iconName;
        public string IconName
        {
            get { return _iconName ?? (Action != null ? Action.Icon : null); }
            set { _iconName = value; }
        }

        public string IconUrl { get; set; }

        #endregion

        public string ParameterString { get; set; }

        private object _parameters;
        public object Parameters
        {
            get { return _parameters ?? (_parameters = ActionFramework.ParseParameters(ReplaceTokens(ParameterString))); }
            set { _parameters = value; }
        }

        private bool _iconVisible = true;
        public bool IconVisible
        {
            get { return _iconVisible; }
            set { _iconVisible = value; }
        }

        private int _iconSize = 16;
        public int IconSize
        {
            get { return _iconSize; }
            set { _iconSize = value; }
        }

        public bool UseContentIcon { get; set; }

        public bool? IncludeBackUrl { get; set; }

        public bool CheckActionCount { get; set; }

        private ContextBoundPortlet _ctxBoundPortlet;
        protected ContextBoundPortlet ContainingContextBoundPortlet
        {
            get
            {
                return _ctxBoundPortlet ?? (_ctxBoundPortlet = ContextBoundPortlet.GetContainingContextBoundPortlet(this));
            }
        }

        //======================================================== Overrides

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //hide action link if action is empty
            if (Action == null)
                this.Visible = false;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            //render nothing if the action does not exist
            if (Action == null)
                return;

            var actionClickable = true;
            if (CheckActionCount)
            {
                var am = ActionMenu.FindContainerActionMenu(this);
                var actionCount = 0;
                var scenario = string.Empty;

                if (am != null)
                {
                    scenario = am.Scenario;

                    if (!string.IsNullOrEmpty(scenario))
                    {
                        var sc = ScenarioManager.GetScenario(scenario);
                        if (sc != null)
                        {
                            actionCount = sc.GetActions(Content.Load(ContextPath), null).Count();
                        }
                    }
                }

                if (actionCount > 1) // && string.Equals(scenario, "new", StringComparison.CurrentCultureIgnoreCase))
                {
                    actionClickable = false;
                }
            }

            if (actionClickable)
            {
                var claction = Action as ClientAction;
                if (claction != null && claction.Callback != null)
                {
                    NavigateUrl = "javascript:";
                    this.Attributes["onclick"] = claction.Callback;
                }
                else
                {
                    NavigateUrl = Action.Uri;
                }
            }

            this.CssClass += (string.IsNullOrEmpty(this.CssClass) ? "" : " ") + "sn-actionlinkbutton";

            if (Action.Forbidden)
            {
                this.CssClass += " sn-disabled";
                this.Enabled = false;
                this.NavigateUrl = string.Empty;
            }

            base.Render(writer);
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            base.RenderBeginTag(writer);

            if (!IconVisible) 
                return;

            if (UseContentIcon && this.Action != null)
                IconName = this.Action.GetContent().Icon;

            writer.Write(!string.IsNullOrEmpty(IconUrl)
                             ? IconHelper.RenderIconTagFromPath(IconUrl, IconSize, this.ToolTip)
                             : IconHelper.RenderIconTag(IconName, null, IconSize, this.ToolTip));
        }

        //======================================================== Internals

        private string _contextPath;
        protected virtual string ContextPath
        {
            get
            {
                if (string.IsNullOrEmpty(_contextPath))
                {
                    var context = UITools.FindContextInfo(this, ContextInfoID);
                    if (context != null)
                    {
                        _contextPath = context.Path;

                        //NodePath may contain a relative path
                        if (!string.IsNullOrEmpty(NodePath))
                        {
                            _contextPath = RepositoryPath.Combine(_contextPath, NodePath);
                        }
                    }
                    else if (!string.IsNullOrEmpty(NodePath))
                        _contextPath = NodePath;
                    else
                        _contextPath = ActionMenu.GetPathFromContentView(Parent);
                }

                return _contextPath ?? string.Empty;
            }
        }

        private bool _actionChecked;
        private ActionBase _action;
        public ActionBase Action
        {
            get
            {
                if (!_actionChecked && _action == null && !string.IsNullOrEmpty(ActionName))
                {
                    _actionChecked = true;

                    try
                    {
                        var nodeHead = NodeHead.Get(ContextPath);
                        if (nodeHead == null || !SecurityHandler.HasPermission(nodeHead, PermissionType.See, PermissionType.Open))
                            return null;

                        var c = Content.Load(ContextPath);
                        if (c != null)
                        {
                            _action = ActionFramework.GetAction(ActionName, c, Parameters);

                            if (_action != null && this.IncludeBackUrl.HasValue)
                                _action.IncludeBackUrl = IncludeBackUrl.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }
                }

                return _action;
            }
        }

        //======================================================== Helper methods

        public void Reset()
        {
            _action = null;
            _actionChecked = false;
            this.Visible = true;
        }

        private string ReplaceTokens(string parameterString)
        {
            var result = parameterString;

            //TODO: refactor parameter token replacement!
            if (!string.IsNullOrEmpty(result))
            {
                if (result.Contains("{PortletClientID}"))
                {
                    if (ContainingContextBoundPortlet != null)
                        result = result.Replace("{PortletClientID}", "PortletClientID=" + ContainingContextBoundPortlet.ClientID);
                }

                if (result.Contains("{CurrentContextPath}"))
                {
                    var ctxPath = string.Empty;

                    if (ContainingContextBoundPortlet != null)
                        ctxPath = ContainingContextBoundPortlet.ContextNode.Path;
                    else if (PortalContext.Current != null)
                        ctxPath = PortalContext.Current.ContextNodePath;
                    
                    result = result.Replace("{CurrentContextPath}", ctxPath);
                }
            }

            return result;
        }
    }
}
