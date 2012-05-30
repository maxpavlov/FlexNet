using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class WorkspaceSummaryPortlet : ContentCollectionPortlet
    {
        private const string KPIViewPath = "/Root/Global/renderers/KPI/WorkspaceSummary";

        //====================================================================== Constructor

        public WorkspaceSummaryPortlet()
        {
            Name = "Workspace Summary";
            Description = "A portlet for displaying list of workspaces and Key Performance Indicators";
            Category = new PortletCategory(PortletCategoryType.KPI);

            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Collection, EditorCategory.ContextBinding };
            this.HiddenProperties = new List<string>() { "SkinPreFix", "Renderer" };
        }

        //====================================================================== Properties

        private int _daysMediumWarning = 5;

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Medium warning days")]
        [WebDescription("If a workspace has not changed for the given days, the indicators will show a medium warning.")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(130)]
        public int DaysMediumWarning
        {
            get { return _daysMediumWarning; }
            set { _daysMediumWarning = value; }
        }

        private int _daysHighWarning = 20;

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Strong warning days")]
        [WebDescription("If a workspace has not changed for the given days, the indicators will show a strong warning (please give a higher number than for medium warning).")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(140)]
        public int DaysHighWarning
        {
            get { return _daysHighWarning; }
            set { _daysHighWarning = value; }
        }

        private string _viewName;

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Summary KPI view")]
        [WebDescription("Select a summary KPI view for rendering portlet output")]
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
            smartFolder.Query = string.Format("+InTree:\"{0}\" +TypeIs:Workspace -TypeIs:(Blog Wiki)", contextNode.Path);

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
