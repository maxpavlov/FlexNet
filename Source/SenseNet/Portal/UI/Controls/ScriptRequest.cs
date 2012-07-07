using System;
using System.ComponentModel;
using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    [DefaultProperty("Path")]
    [ToolboxData("<{0}:ScriptRequest runat=server></{0}:ScriptRequest>")]
    public class ScriptRequest : Control
    {
        [Bindable(true)]
        [DefaultValue("")]
        public string Path
        {
            get
            {
                String s = (String)ViewState["Path"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Path"] = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            UITools.AddScript(Path);
            base.OnInit(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            return;
        }
    }
}
