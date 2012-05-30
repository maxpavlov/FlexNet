using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Workspaces;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets
{
    public class WorkspaceKPIPortlet : CacheablePortlet
    {
        /* ====================================================================================================== Constants */
        private const string kpiViewPath = "/Root/Global/renderers/KPI/WorkspaceKPI";


        /* ====================================================================================================== Properties */
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("KPI View")]
        [WebDescription("The .ascx KPI View to be presented. Views are defined in the KPI/WorkspaceKPI folder under /Root/Global/renderers.")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(DropDownPartField), typeof(IEditorPartField))]
        [DropDownPartOptions("InFolder:\"" + kpiViewPath + "\"")]
        public string ViewName { get; set; }


        /* ====================================================================================================== Constructor */
        public WorkspaceKPIPortlet()
        {
            this.Name = "Workspace KPI";
            this.Description = "A portlet for presenting a KPI value with a selected KPI View";
            this.Category = new PortletCategory(PortletCategoryType.KPI);

            this.HiddenProperties = new List<string>() {"SkinPreFix", "Renderer"};
        }


        /* ====================================================================================================== Methods */
        protected override void CreateChildControls()
        {
            // check if placed under a workspace
            if (!(PortalContext.Current.ContextNode is Workspace))
            {
                this.Controls.Clear();
                this.Controls.Add(new Label() {Text = "This portlet is only operational in a workspace context!"});
                return;
            }

            // load view
            UserControl view = null;
            if (!string.IsNullOrEmpty(this.ViewName))
                view = Page.LoadControl(RepositoryPath.Combine(kpiViewPath, this.ViewName)) as UserControl;

            if (view != null)
                this.Controls.Add(view);
            else
                this.Controls.Add(new Label() { Text = "No KPI view is loaded" });


            this.ChildControlsCreated = true;
        }
    }
}
