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
    [ToolboxData("<{0}:SkinResolver runat=server></{0}:SkinResolver>")]
    public class SkinResolver : Control
    {
        public string RelPath { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            var path = SkinManager.Resolve(RelPath);
            writer.Write(path);
        }
    }
}
