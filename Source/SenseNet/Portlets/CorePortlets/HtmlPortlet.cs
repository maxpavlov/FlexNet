using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.UI.PortletFramework;
using System.ComponentModel;

namespace SenseNet.Portal.Portlets
{
    public class HtmlPortlet : PortletBase
    {
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("HTML")]
        [WebDescription("Raw HTML fragment")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string Html { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        
        public HtmlPortlet()
        {
            this.Name = "HTML source";
            this.Description = "For special effects, insert raw HTML source into the page as a portlet";
            this.Category = new PortletCategory(PortletCategoryType.Application);
        }
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(Html);
        }
    }
}
