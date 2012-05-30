using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class WorkspaceAggregatedKPIPortlet : ContentCollectionPortlet
    {
        private const string KPIViewPath = "/Root/Global/renderers/KPI/WorkspaceAggregatedKPI";

        //====================================================================== Constructor

        public WorkspaceAggregatedKPIPortlet()
        {
            Name = "Workspace Aggregated KPI";
            Description = "A portlet for displaying list of workspaces and Key Performance Indicators";
            Category = new PortletCategory(PortletCategoryType.Collection);

            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Collection, EditorCategory.ContextBinding };
            this.HiddenProperties = new List<string>() { "SkinPreFix", "Renderer" };
        }

        //====================================================================== Properties


        private string _viewName;

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Aggregated KPI view")]
        [WebDescription("Select an aggregated KPI view for rendering portlet output")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(DropDownPartField), typeof(IEditorPartField))]
        [DropDownPartOptions("InFolder:\"" + KPIViewPath + "\"")]
        public string ViewName
        {
            get { return _viewName; }
            set
            {
                _viewName = value;

                this.Renderer = RepositoryPath.Combine(KPIViewPath, _viewName ?? string.Empty);
            }
        }

        //====================================================================== Model

        protected override object GetModel()
        {
            var contextNode = GetContextNode();
            if (contextNode == null)
                return null;

            var smartFolder = Node.Load<SmartFolder>("/Root/System/RuntimeQuery");
            if (smartFolder == null)
            {
                using (new SystemAccount())
                {
                    var systemFolder = Node.LoadNode("/Root/System");
                    smartFolder = new SmartFolder(systemFolder) { Name = "RuntimeQuery" };
                    smartFolder.Save();
                }
            }

            var content = Content.Create(smartFolder);
            smartFolder.Query = string.Format("+InTree:\"{0}\" +TypeIs:Workspace -TypeIs:(Blog Wiki Site)", contextNode.Path);

            var baseModel = base.GetModel() as Content;
            if (baseModel != null)
            {
                content.ChildrenQueryFilter = baseModel.ChildrenQueryFilter;
                content.ChildrenQuerySettings = baseModel.ChildrenQuerySettings;
            }

            return content;
        }
    }
}
