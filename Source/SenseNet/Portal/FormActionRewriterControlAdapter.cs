using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace SenseNet.Portal
{

    /// <summary>
    /// This control adapter uses the FormActionRewriterHtmlTextWriter class for rendering instead of the default HtmlTextWriter.
    /// </summary>
    public class FormActionRewriterControlAdapter : System.Web.UI.Adapters.ControlAdapter
    {
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(new FormActionRewriterHtmlTextWriter(writer));
        }
    }



    /// <summary>
    /// Modifies the standard HtmlTextWriter to render the original raw URL in the the action attribute of the HTML forms.
    /// This is the right behaviour when the system rewrites the request URLs (eg. using friendrly URLs).
    /// </summary>
    public class FormActionRewriterHtmlTextWriter : HtmlTextWriter
    {
        public FormActionRewriterHtmlTextWriter(System.IO.TextWriter writer)
            : base(writer)
        {
            InnerWriter = writer;
        }

        public FormActionRewriterHtmlTextWriter(HtmlTextWriter writer)
            : base(writer)
        {
            InnerWriter = writer.InnerWriter;
        }


        public override void WriteAttribute(string name, string value, bool fEncode)
        {
            if (name == "action")
                value = HttpContext.Current.Request.RawUrl;

            base.WriteAttribute(name, value, fEncode);
        }
    }

}