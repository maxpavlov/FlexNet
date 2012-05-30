using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class TextEditorPartField : TextBox, IEditorPartField
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


        /* ====================================================================================================== Properties */
        private TextEditorPartOptions _textEditorOptions;
        public TextEditorPartOptions TextEditorOptions
        {
            get
            {
                if (_textEditorOptions == null)
                {
                    _textEditorOptions = this.Options as TextEditorPartOptions;
                    if (_textEditorOptions == null)
                        _textEditorOptions = new TextEditorPartOptions();
                }
                return _textEditorOptions;
            }
        }


        /* ====================================================================================================== Methods */
        protected override void Render(HtmlTextWriter writer)
        {
            this.TextMode = TextEditorOptions.TextMode;
            this.Rows = TextEditorOptions.Rows;
            this.Columns = TextEditorOptions.Columns;
            this.MaxLength = TextEditorOptions.MaxLength;

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