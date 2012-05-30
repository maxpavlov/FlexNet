using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;

namespace SenseNet.Portal.Portlets
{
    public class DialogFileUploadPortlet : ContextBoundPortlet
    {
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Allowed Content Types")]
        [WebDescription("The comma separated list of allowed Content Type names for the target upload folder to be created.")]
        [WebCategory("Dialog File Upload", 100)]
        [WebOrder(100)]
        public string AllowedContentTypes { get; set; }

        public DialogFileUploadPortlet()
        {
            this.Name = "Dialog File Upload";
            this.Description = "A simple upload portlet displaying a fileupload control and previously uploaded files";
            this.Category = new PortletCategory(PortletCategoryType.Application);
        }

        protected override void OnInit(EventArgs e)
        {
            var control = this.Page.LoadControl("/Root/System/SystemPlugins/Controls/DialogFileUpload.ascx") as SenseNet.Portal.UI.Controls.DialogFileUpload;
            control.AllowedContentTypes = this.AllowedContentTypes;
            this.Controls.Add(control);
            base.OnInit(e);
        }
    }
}
