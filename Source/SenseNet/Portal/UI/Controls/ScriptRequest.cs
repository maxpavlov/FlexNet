using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
    [DefaultProperty("Path")]
    [ToolboxData("<{0}:ScriptRequest runat=server></{0}:ScriptRequest>")]
    public class ScriptRequest : WebControl
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
    }
}
