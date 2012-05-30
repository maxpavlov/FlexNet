using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ApplicationModel;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:ActionButton ID=\"ActionButton1\" runat=server></{0}:ActionButton>")]
    public class ActionButton : Button, IActionUiAdapter
    {
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            
            var context = UITools.FindContextInfo(this, ContextInfoID);
            OnClientClick = context == null 
                ? GetOnClientClick(NodePath, ActionName) 
                : GetOnClientClick(context.Path, ActionName);

            this.CssClass += (string.IsNullOrEmpty(this.CssClass) ? "" : " ") + "sn-actionbutton";
        }

        private static string GetOnClientClick(string nodePath, string actionName)
        {
            var result = new StringBuilder();
            result.Append(@"javascript: window.location = '");
            result.Append(ActionFramework.GetActionUrl(nodePath, actionName, HttpUtility.UrlEncode(HttpContext.Current.Request.RawUrl)));
            result.Append(@"';");
            result.Append(@"return false;");
            return result.ToString();
        }        
       
        #region IActionUiAdapter Members

        public string NodePath { get; set; }
        public string ContextInfoID { get; set; }
        public string WrapperCssClass { get; set; }
        public string Scenario { get; set; }
        public string ScenarioParameters { get; set; }
        public string ActionName { get; set; }
        public string IconName { get; set; }
        public string IconUrl { get; set; }

        #endregion
    }
}