using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class PropertyFieldPanel : WebControl
    {
        // Properties /////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        // Events /////////////////////////////////////////////////////////////////
        
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            RenderBeginTagInternal(writer, Title, this.ClientID);
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
            RenderEndTagInternal(writer);
        }

        protected virtual void RenderHeader(HtmlTextWriter writer)
        {
        }

        protected virtual void RenderFooter(HtmlTextWriter writer)
        {
            RenderFooterInternal(writer);
        }

        protected virtual void RenderTitle(HtmlTextWriter writer)
        {
            RenderTitleInternal(writer, Title, this.ClientID);
        }

        protected override void OnPreRender(EventArgs e)
        {
            UITools.RegisterStartupScript("accordions", "SN.PortalRemoteControl.InitPortletEditorAccordion()", Page);
            base.OnPreRender(e);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            RenderBeginTag(writer);
            RenderContents(writer);
            RenderEndTag(writer);
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            RenderContentsStart(writer);
            base.RenderContents(writer);
            RenderContentsEnd(writer);
        }

        // Staic methods //////////////////////////////////////////////////////////
        internal static void RenderBeginTagInternal(HtmlTextWriter writer, string title, string id)
        {
            RenderHeaderInternal(writer, title, id);
        }

        internal static void RenderEndTagInternal(HtmlTextWriter writer)
        {
            RenderFooterInternal(writer);
        }

        internal static void RenderHeaderInternal(HtmlTextWriter writer, string title, string id)
        {
            RenderTitleInternal(writer, title, id);
            writer.Write(@"<div id='" + id + @"' class=""sn-accordion-content"">");
        }

        internal static void RenderFooterInternal(HtmlTextWriter writer)
        {
            writer.Write("</div>");
        }

        internal static void RenderTitleInternal(HtmlTextWriter writer, string title, string id)
        {
            writer.Write(String.Format(@"<h3 class=""sn-accordion-title""><a href='#" + id + "'>{0}</a></h3>", title));

        }

        internal static void RenderContentsStart(HtmlTextWriter writer)
        {
        }

        internal static void RenderContentsEnd(HtmlTextWriter writer)
        {
        }
    }
}