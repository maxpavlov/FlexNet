using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace SenseNet.Portal.UI.Controls
{
    public class AdvancedPanelButton : UserControl
    {
        public Control AdvancedPanel
        {
            get; set;
        }

        private string _advancedPanelId;
        public string AdvancedPanelId
        {
            get
            {
                return AdvancedPanel != null ? AdvancedPanel.ClientID : _advancedPanelId;
            }
            set { _advancedPanelId = value; }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var outerPanel = this.FindControlRecursive("AdvancedButtonOuter") as HtmlControl;
            var hidePanel = this.FindControlRecursive("HidePanel") as HtmlControl;
            var showPanel = this.FindControlRecursive("ShowPanel") as HtmlControl;

            if (outerPanel != null && hidePanel != null && showPanel != null)
            {
                outerPanel.Attributes["onclick"] = string.Format("SN.Util.ToggleAdvancedPanel('{0}', '{1}', '{2}')", showPanel.ClientID, hidePanel.ClientID, AdvancedPanelId);
            }

            base.Render(writer);
        }
    }
}
