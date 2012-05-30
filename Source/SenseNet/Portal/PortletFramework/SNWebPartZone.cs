using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.WebControls;

[assembly: TagPrefix("SenseNet.Portal.UI.PortletFramework", "sn")]
namespace SenseNet.Portal.UI.PortletFramework
{
    public class SNWebPartZone : WebPartZone
    {
        //========================================================================================== Properties

        protected bool IsDesignMode { get; set; }

        //========================================================================================== Overrides

        public override string EmptyZoneText
        {
            get
            {
                return HttpContext.GetGlobalResourceObject("Portal", "WebPartEmptyZoneText") as string; ;
            }
        }

        protected override WebPartChrome CreateWebPartChrome()
        {
            //We ovverride the default chrome implementation with a tableless version.
            return new SNWebPartChrome(this, this.WebPartManager);
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            // RenderBeginTag is overridden: creates a div instead of a table tag. 
            // It adds a class to the div that is used by the webpart javascript for drag ad drop.
            // The classes can also be used to customize the general look and feel of the zones.
            // Classes:
            //   sn-zone               -  it is added if the zone is not empty
            //   sn-zone-hide          -  it is added if the zone is emtpy and the page is in BrowseDisplayMode
            //   sn-zone sn-zone-empty -  it is added if the zone is empty, but the page is in Design or EditDisplayMode mode

            var zoneClass = GetZoneClass();
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, zoneClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }

        protected override void RenderHeader(HtmlTextWriter writer)
        {
            // Zone header contains the name of the zone and the "Add portlet" link

            //<span class="sn-zone-head">{0}</span>
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-zone-head");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(this.DisplayTitle);

            string picker = "<a href='#' class='sn-addportlet' data-zone='"+this.ID+"'>Add portlet</a>";
            writer.Write(picker);

            writer.RenderEndTag();
        }
        
        protected override void RenderContents(HtmlTextWriter writer)
        {
            //We override the RenderContents method to avoid the table tags.
            //Creating the appropriate divs is implemented in the RenderHeader and RenderBody methods.

            if (HasHeader)
                RenderHeader(writer);

            RenderBody(writer);

            if (HasFooter)
                RenderFooter(writer);
        }
        
        protected override void RenderBody(HtmlTextWriter writer)
        {
            if (this.WebPartManager.DisplayMode.Name == "Design" || this.WebPartManager.DisplayMode.Name == "Edit")
                this.IsDesignMode = true;

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-zone-body");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            this.RenderEmptyZoneBody(writer);
            if (this.WebParts.Count != 0) {
                foreach (WebPart webPart in WebParts)
                {
                    WebPartChrome.RenderWebPart(writer, webPart);
                }
            }

            writer.RenderEndTag();
        }

        //========================================================================================== Helper methods

        private void RenderEmptyZoneBody(HtmlTextWriter writer)
        {
            string emptyZoneText = this.EmptyZoneText;
            bool designMode = ((!this.DesignMode && this.AllowLayoutChange)
            && ((this.WebPartManager != null)
            && this.WebPartManager.DisplayMode.AllowPageDesign))
            && !string.IsNullOrEmpty(emptyZoneText);

            if (designMode)
            {
                var emptyZoneTextStyle = EmptyZoneTextStyle;
                if (!emptyZoneTextStyle.IsEmpty)
                    emptyZoneTextStyle.AddAttributesToRender(writer, this);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-zone-empty-text");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(emptyZoneText);
                writer.RenderEndTag();
            }
        }
        
        private string GetZoneClass()
        {
            var currWebPartManager = WebPartManager.GetCurrentWebPartManager(this.Page);
            var currDisplayMode = currWebPartManager == null ? WebPartManager.BrowseDisplayMode : currWebPartManager.DisplayMode;

            if (currDisplayMode == WebPartManager.BrowseDisplayMode)
                return (this.WebParts.Count != 0) ? "sn-zone" : "sn-zone-hide";

            return (this.WebParts.Count != 0) ? "sn-zone sn-zone-edit" : "sn-zone sn-zone-edit sn-zone-empty";
        }

    }

}