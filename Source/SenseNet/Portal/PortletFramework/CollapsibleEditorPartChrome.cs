using System;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class CollapsibleEditorPartChrome : EditorPartChrome
    {
        // Members and properties /////////////////////////////////////////////////
        public CollapsibleEditorPartChrome(EditorZoneBase zone) : base(zone) { }

        CollapsibleEditorZone EditorZone
        {
            get { return (CollapsibleEditorZone)this.Zone; }
        }

        // Events /////////////////////////////////////////////////////////////////
        public override void RenderEditorPart(HtmlTextWriter writer, EditorPart editorPart)
        {
            if (editorPart == null)
                throw new ArgumentNullException("editorPart");

            RenderEditorPartInternal(writer, editorPart);
        }

        // Internals //////////////////////////////////////////////////////////////
        private void RenderEditorPartInternal(HtmlTextWriter writer, EditorPart editorPart)
        {
            if (editorPart == null)
                throw new ArgumentNullException("editorPart");

            var editorName = editorPart.GetType().Name;
            switch(editorName)
            {
                case "PropertyGridEditorPart":
                case "BehaviorEditorPart":
                case "AppearanceEditorPart":
                case "LayoutEditorPart":
                    break;
                case "PropertyEditorPart" :
                    var chromeType = this.Zone.GetEffectiveChromeType(editorPart);
                    var style = this.CreateEditorPartChromeStyle(editorPart, chromeType);
                    if (!style.IsEmpty)
                        style.AddAttributesToRender(writer, this.Zone);
                    //if ((chromeType == PartChromeType.TitleAndBorder) || (chromeType == PartChromeType.TitleOnly))
                    //    this.RenderTitle(writer, editorPart);
                    if (editorPart.ChromeState != PartChromeState.Minimized)
                        RenderPartContentsInternal(writer, editorPart);                        
                    break;
                default:
                    //
                    //  render custom editorparts
                    //
                    this.RenderPartContents(writer, editorPart);
                    break;
            }

        }
        private void RenderPartContentsInternal(HtmlTextWriter writer, EditorPart editorPart)
        {
            if (editorPart == null) 
                throw new ArgumentNullException("editorPart");
                
            Style style2 = this.Zone.PartStyle;
            var editorName = editorPart.GetType().Name;

            if (!style2.IsEmpty)
                style2.AddAttributesToRender(writer, this.Zone);

            AddAttributes(writer, editorPart);
            
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            if (!editorName.Equals("PropertyEditorPart"))
            {
                PropertyFieldPanel.RenderBeginTagInternal(writer, editorPart.Title, editorPart.ID); 
                PropertyFieldPanel.RenderContentsStart(writer);                
            }
            this.RenderPartContents(writer, editorPart);
            if (!editorName.Equals("PropertyEditorPart"))
            {
                PropertyFieldPanel.RenderContentsEnd(writer);
                PropertyFieldPanel.RenderEndTagInternal(writer);
            }
            writer.RenderEndTag();

            this.EditorZone.EditorPartsAdded = true;
        }
        private static void AddAttributes(HtmlTextWriter writer, WebControl editorPart)
        {
            if (editorPart == null)
                throw new ArgumentNullException("editorPart");

            var cssClass = String.IsNullOrEmpty(editorPart.CssClass) ? "snAccordionBody" : String.Concat(editorPart.CssClass, " ", "snAccordionBody");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
            writer.AddAttribute("id", "EditorPartBody_" + editorPart.ClientID);
        }

        
    }

}
