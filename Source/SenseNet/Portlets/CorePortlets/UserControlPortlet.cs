using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI;

namespace SenseNet.Portal.Portlets
{
    public class UserControlPortlet : PortletBase
    {
        public UserControlPortlet()
        {
            this.Name = "User control";
            this.Description = "A portlet for rendering an ASP.NET User Control";
            this.Category = new PortletCategory(PortletCategoryType.Application);

            this.HiddenProperties.Add("Renderer");
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("User Control path")]
        [WebDescription("Path of the .ascx user control which provides the elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ControlPath { get; set; }


        protected override void CreateChildControls()
        {
            if (RenderingMode == RenderMode.Native)
            {
                Controls.Clear();

                try
                {
                    var c = CreateViewControl(ControlPath);

                    c.ID = this.ClientID + "_userControlPortlet";
                    Controls.Add(c);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);

                    this.Controls.Add(new LiteralControl(ex.Message));
                }
            }
            ChildControlsCreated = true;
        }

        private Control CreateViewControl(string path)
        {
            if (!string.IsNullOrEmpty(path))
                return Page.LoadControl(path);
            return new Control();
        }
    }
}
