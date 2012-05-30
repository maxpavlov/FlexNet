using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web;
using System.ComponentModel;
using System.Web.Script.Serialization;
using SenseNet.ContentRepository.Storage.ApplicationMessaging;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;

using SenseNet.ContentRepository;
using SenseNet.Portal.Handlers;

namespace SenseNet.Portal.UI.Controls
{
    public class ListViewUpload : Upload
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //UITools.AddExtCssFiles();
            //UITools.AddSnUxScripts();
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.DesignMode) return;

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-toolbar");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-toolbar-inner");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            this.RenderUploadButton(writer);
            if (AllowOtherContentType) 
                RenderContentTypeDropDown(writer);
            this.RenderCancelButton(writer);

            writer.RenderEndTag();
            writer.RenderEndTag();

            this.RenderProgressBar(writer);

            if (!this._isEmpty) return;
            this.RenderEmptyEntry(writer);
            this.RenderFileInfo(writer);

            //StringBuilder sb = new StringBuilder();
            //sb.Append("var sm = sn.ux.smartMenu;");
            //sb.Append("$('.sn-toolbar-button').hover(sm.showArrow, sm.hideArrow);");
            //Page.ClientScript.RegisterStartupScript(typeof(Page), "empty7", sb.ToString(), true);
        }

        protected override void RenderUploadButton(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-toolbar-button");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            base.RenderUploadButton(writer);
            writer.RenderEndTag();
        }

        protected override void RenderCancelButton(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-toolbar-button");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            base.RenderCancelButton(writer);
            writer.RenderEndTag();
        }

    }
}
