using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Search;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.Portlets
{
    public class KPIViewDropDownPartField : TextBox, IEditorPartField
    {
        /* ====================================================================================================== Constants */


        /* ====================================================================================================== IEditorPartField */
        private EditorOptions _options;
        public EditorOptions Options
        {
            get
            {
                return _options;
            }
            set
            {
                _options = value;
            }
        }
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
        private DropDownPartOptions _dropdownOptions;
        public DropDownPartOptions DropdownOptions
        {
            get
            {
                if (_dropdownOptions == null)
                {
                    _dropdownOptions = this.Options as DropDownPartOptions;
                    if (_dropdownOptions == null)
                        _dropdownOptions = new DropDownPartOptions();
                }
                return _dropdownOptions;
            }
        }


        /* ====================================================================================================== Methods */
        protected override void Render(HtmlTextWriter writer)
        {
            var clientId = String.Concat(ClientID, "Div");
            string htmlPart2 = @"<div class=""{0}"" id=""{1}"">";
            writer.Write(String.Format(htmlPart2, EditorPartCssClass, clientId));
            RenderTitle(writer);


            // render dropdown
            var controlCss = ControlWrapperCssClass;
            if (!string.IsNullOrEmpty(this.DropdownOptions.CustomControlCss))
                controlCss = string.Concat(controlCss, " ", this.DropdownOptions.CustomControlCss);

            writer.Write(String.Format(@"<div class=""{0}"">", controlCss));
            writer.Write("<select></select>");


            // render textbox
            writer.Write(@"<div style=""display:none;"">");
            base.Render(writer);
            writer.Write("</div>");
            writer.Write("</div>");

            writer.Write("</div>");

        }
    }
}
