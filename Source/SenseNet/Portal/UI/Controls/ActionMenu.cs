using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository;
using SenseNet.Portal.PortletFramework;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.ComponentModel;
using System.Linq;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage;
using Content = SenseNet.ContentRepository.Content;

[assembly: WebResource("SenseNet.Portal.UI.Controls.ActionMenu.js", "application/x-javascript")]
namespace SenseNet.Portal.UI.Controls
{
    public enum ActionMenuMode
    {
        Default = 0,
        Text = 1,
        Link = 2,
        Split = 3
    }
    [ToolboxData("<{0}:ActionMenu ID=\"ActionMenu1\" runat=server></{0}:ActionMenu>")]
    public class ActionMenu : Label, IScriptControl, IActionUiAdapter
    {
        // Members /////////////////////////////////////////////////////

        public string ServiceUrl { get; set; }
        public string Href { get; set; }
        [DefaultValue(typeof(ActionMenuMode), "Default")]
        public ActionMenuMode Mode { get; set; }
        public string LoadingText { get; set; }
        public string ItemHoverCssClass { get; set; }
        public bool CheckActionCount { get; set; }
        public string RequiredPermissions { get; set; }
        protected bool ClickDisabled { get; set; }

        protected Content Content { get; set; }

        // Events //////////////////////////////////////////////////////

        protected override void OnInit(EventArgs e)
        {
            UITools.AddPickerCss();

            UITools.AddScript(UITools.ClientScriptConfigurations.MSAjaxPath);
            UITools.AddScript(UITools.ClientScriptConfigurations.jQueryPath);
            UITools.AddScript(UITools.ClientScriptConfigurations.SNWebdavPath);
            UITools.AddScript(UITools.ClientScriptConfigurations.SNPickerPath);
            UITools.AddScript(UITools.ClientScriptConfigurations.SNWallPath);

            base.OnInit(e);
        }

        // Rendering //////////////////////////////////////////////////

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            if (ClickDisabled)
                return;

            var wrapperCssClass = PrepareWrapperCssClass();

            writer.Write(String.Format(@"<span id=""{1}"" class=""{0}"">", wrapperCssClass, ClientID));

            if (!string.IsNullOrEmpty(CssClass))
                CssClass = "sn-actionmenu-inner ui-state-default ui-corner-all " + CssClass;
            else
                CssClass = "sn-actionmenu-inner ui-state-default ui-corner-all";

            base.RenderBeginTag(writer);

            var title = string.Empty;
            var overlay = OverlayVisible ? IconHelper.GetOverlay(this.Content, out title) : string.Empty;

            if (!String.IsNullOrEmpty(IconUrl))
                writer.Write(IconHelper.RenderIconTagFromPath(IconUrl, overlay, 16, title));
            else if (!String.IsNullOrEmpty(IconName))
                writer.Write(IconHelper.RenderIconTag(IconName, overlay, 16, title));
        }
        
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (ClickDisabled)
                return;

            base.RenderEndTag(writer);
            
            writer.Write("</span>");
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!ClickDisabled)
            {
                var page = ScriptManager.GetCurrent(Page);
                if (page != null)
                    page.RegisterScriptControl(this);
            }

            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (!ClickDisabled)
            {
                var page = ScriptManager.GetCurrent(Page);
                if (page != null)
                    page.RegisterScriptDescriptors(this);
            }

            base.Render(writer);
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            SetServiceUrl();
        }
        
        // IScriptControl members /////////////////////////////////////
        
        /// <summary>
        /// Gets a collection of script descriptors that represent ECMAScript (JavaScript) client components.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerable"/> collection of <see cref="T:System.Web.UI.ScriptDescriptor"/> objects.
        /// </returns>
        public IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var descriptor = new ScriptControlDescriptor("SenseNet.Portal.UI.Controls.ActionMenu", ClientID);
            descriptor.AddProperty("ServiceUrl", ServiceUrl);
            descriptor.AddProperty("WrapperCssClass", PrepareWrapperCssClass());
            descriptor.AddProperty("Mode", GetModeName);
            descriptor.AddProperty("ItemHoverCssClass", ItemHoverCssClass);
            yield return descriptor;
        }
        /// <summary>
        /// Gets a collection of <see cref="T:System.Web.UI.ScriptReference"/> objects that define script resources that the control requires.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerable"/> collection of <see cref="T:System.Web.UI.ScriptReference"/> objects.
        /// </returns>
        public IEnumerable<ScriptReference> GetScriptReferences()
        {
            yield return new ScriptReference("SenseNet.Portal.UI.Controls.ActionMenu.js", GetType().Assembly.FullName);
        }

        // IActionUiAdapter members ////////////////////////////////////////
        
        [PersistenceMode(PersistenceMode.Attribute)]
        public string NodePath { get; set; }
        public string ContextInfoID { get; set; }        
        public string WrapperCssClass { get; set; }
        public string Scenario { get; set; }
        public string ScenarioParameters { get; set; }
        public string ActionName { get; set; }
        public string IconName { get; set; }
        public string IconUrl { get; set; }
        public bool OverlayVisible { get; set; }
        
        // Internals //////////////////////////////////////////////////
        
        private string GetModeName
        {
            get { return Enum.GetName(typeof(ActionMenuMode), Mode).ToLower(); }
        }
        private string PrepareWrapperCssClass()
        {
            var enumName = Enum.GetName(typeof(ActionMenuMode), Mode);
            return string.Concat("sn-actionmenu sn-actionmenu-", enumName.ToLower(), "-mode ui-widget", string.IsNullOrEmpty(WrapperCssClass) ? "" : String.Concat(" ", WrapperCssClass));
        }
        /// <summary>
        /// Sets the callback URL of the ActionMenu. It represents the service url with correct parameters for the actions.
        /// </summary>
        private void SetServiceUrl()
        {
            var scParams = GetReplacedScenarioParameters();
            var context = UITools.FindContextInfo(this, ContextInfoID);
            var path = !String.IsNullOrEmpty(ContextInfoID) ? context.Path : NodePath;

            var encodedReturnUrl = Uri.EscapeDataString(PortalContext.Current.RequestedUri.PathAndQuery);
            var encodedParams = Uri.EscapeDataString(scParams ?? string.Empty);

            if (String.IsNullOrEmpty(path))
                path = GetPathFromContentView(this);

            if (string.IsNullOrEmpty(path))
            {
                this.Visible = false;
                return;
            }

            this.Content = Content.Load(path);

            //Pre-check action count. If empty, hide the action menu.
            if (CheckActionCount)
            {
                var sc = ScenarioManager.GetScenario(Scenario, scParams);
                var actionCount = 0;

                if (sc != null)
                    actionCount = sc.GetActions(this.Content, PortalContext.Current.RequestedUri.PathAndQuery).Count();

                if (actionCount < 2 && string.Equals(Scenario, "new", StringComparison.CurrentCultureIgnoreCase))
                {
                    ClickDisabled = true;
                }
                else if (actionCount == 0)
                {
                    this.Visible = false;
                    return;
                }
            }

            //Pre-check required permissions
            var permissions = ActionFramework.GetRequiredPermissions(RequiredPermissions);
            if (permissions.Count > 0 && !SecurityHandler.HasPermission(NodeHead.Get(path), permissions.ToArray()))
            {
                this.Visible = false;
                return;
            }

            var encodedPath = HttpUtility.UrlEncode(path);

            ServiceUrl = String.Format("/SmartAppHelper.mvc/GetActions?path={0}&scenario={1}&back={2}&parameters={3}",
                                            encodedPath, Scenario, encodedReturnUrl, encodedParams);
        }

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

        internal static string GetPathFromContentView(Control parent)
        {
            var control = parent;
            while (control.Parent != null && !(control is SingleContentView))
                control = control.Parent;

            var contentView = control as SingleContentView;
            if (contentView == null)
                return null;

            return contentView.Content.Path;
        }

        public static ActionMenu FindContainerActionMenu(Control control)
        {
            while (true)
            {
                if (control == null)
                    return null;

                var am = control as ActionMenu;
                if (am != null)
                    return am;

                control = control.Parent;
            }
        }
    }
}
