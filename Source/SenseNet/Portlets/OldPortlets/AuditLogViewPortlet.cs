using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class AuditLogViewPortlet : PortletBase
    {
        private string _contentViewPath = "/Root/System/SystemPlugins/Portlets/AuditLog/AuditLogControl.ascx";

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Ciew path")]
        [WebDescription("Path of the .ascx user control which provides the elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ContentViewPath
        {
            get { return _contentViewPath; }
            set { _contentViewPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        public AuditLogViewPortlet()
        {
            this.Name = "Audit log viewer";
            this.Description = "List and filter audit log entries";
            this.Category = new PortletCategory(PortletCategoryType.System);
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            CreateControls();
            ChildControlsCreated = true;
        }

        private void CreateControls()
        {
            try
            {
                var viewControl = this.Page.LoadControl(ContentViewPath);
                Controls.Add(viewControl);
            }
            catch(Exception ex)
            {
                Logger.WriteException(ex);
                this.Controls.Clear();
                this.Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }
    }
}
