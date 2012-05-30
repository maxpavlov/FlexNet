using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.UI.Controls
{
    public class RatingHoverPanel : UserControl
    {
        public List<VoteData.HoverPanelDataItem> RateDs;
        protected Repeater r1;
        protected override void OnPreRender(EventArgs e)
        {
            r1.DataSource = RateDs;
            r1.DataBind();
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.AddAttribute("id", this.ClientID);
            writer.AddAttribute("class", "RatingHoverPanel");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            base.Render(writer);
            writer.RenderEndTag();
        }
    }
}
