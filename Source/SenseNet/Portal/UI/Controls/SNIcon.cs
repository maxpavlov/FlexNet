using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
    public class SNIcon : Control
    {
        public string Icon { get; set; }
        public string Overlay { get; set; }
        public string Size { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(Size))
                writer.Write(IconHelper.RenderIconTag(Icon, Overlay));
            else
                writer.Write(IconHelper.RenderIconTag(Icon, Overlay, Int32.Parse(Size)));
        }
    }
}
