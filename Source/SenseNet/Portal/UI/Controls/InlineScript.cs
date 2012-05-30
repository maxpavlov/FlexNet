using System.Text;
using System.Web.UI;
using System.IO;

namespace SenseNet.Portal.UI.Controls
{
    // http://weblogs.asp.net/infinitiesloop/archive/2007/09/17/inline-script-inside-an-asp-net-ajax-updatepanel.aspx
    public class InlineScript : Control
    {
        protected override void Render(HtmlTextWriter writer)
        {
            var sm = ScriptManager.GetCurrent(Page);
            if (sm == null) 
            { 
                base.Render(writer);
                return; 
            }
            if (sm.IsInAsyncPostBack)
            {
                var sb = new StringBuilder();
                base.Render(new HtmlTextWriter(new StringWriter(sb)));
                ScriptManager.RegisterClientScriptBlock(this, typeof(InlineScript), UniqueID, sb.ToString(), false);
            }
            else 
                base.Render(writer);
        }

    }
}