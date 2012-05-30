using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class CheckBoxEditorPartField : CheckBox, IEditorPartField
    {
        /* ====================================================================================================== IEditorPartField */
        public EditorOptions Options { get; set; }
        public string EditorPartCssClass { get; set; }
        public string TitleContainerCssClass { get; set; }
        public string TitleCssClass { get; set; }
        public string DescriptionCssClass { get; set; }
        public string ControlWrapperCssClass { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PropertyName { get; set; }
        public void RenderTitle(HtmlTextWriter writer)
        {
            writer.Write(String.Format(@"<div class=""{0}""><span class=""{1}"" title=""{5}"">{2}</span><br/><span class=""{3}"">{4}</span></div>", TitleContainerCssClass, TitleCssClass, Title, DescriptionCssClass, Description, PropertyName));
        }
        public void RenderDescription(HtmlTextWriter writer)
        {
        }


        /* ====================================================================================================== Methods */
        protected override void Render(HtmlTextWriter writer)
        {
            var clientId = String.Concat(ClientID, "Div");
            string htmlPart = @"<div class=""{0}"" id=""{1}"">";
            writer.Write(String.Format(htmlPart, EditorPartCssClass, clientId));
            RenderTitle(writer);

            writer.Write(String.Format(@"<div class=""{0}"">", ControlWrapperCssClass));
            base.Render(writer);
            writer.Write("</div>");

            writer.Write("</div>");
        }
    }
}
