using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI;


namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// Portlet for administrating tags.
    /// </summary>
    public class TagAdminPortlet : PortletBase
    {
        /// <summary>
        /// Path of tags stored in Content Repository.
        /// </summary>
        private string tagPath;
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.TagAdmin, EditorCategory.TagAdmin_Order)]
        [WebDisplayName("Path of tags")]
        [WebDescription("Path of the folder containing tags")]
        [WebOrder(10)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        public string Tags
        {
            get
            {
                return tagPath;
            }

            set
            {
                tagPath = value.TrimEnd('/');
            }
        }

        /// <summary>
        /// Path of content view with default value.
        /// </summary>
        private string contentViewPath = "/Root/System/SystemPlugins/Portlets/TagAdmin/TagAdminControl.ascx";

        /// <summary>
        /// Property for path of content view.
        /// </summary>
        /// Gets or sets path of content view.
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the UI elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ContentViewPath
        {
            get { return contentViewPath; }
            set { contentViewPath = value; }
        }

        private string searchPaths = string.Empty;

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Search paths")]
        [WebDescription("Only tags under these paths will be populated (separate with commas)")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(AllowedContentTypes = "Folder;SystemFolder")]
        [WebOrder(100)]
        public string SearchPaths
        {
            get { return searchPaths; }
            set { searchPaths = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        /// <summary>
        /// Overridden method for creating controls.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            CreateControls();
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TagAdminPortlet()
        {
            Name = "Tag admin";
            Description = "Portlet for administrating tags";
            Category = new PortletCategory(PortletCategoryType.Application);
            Tags = "/Root/System/Tags";
            UITools.AddStyleSheetToHeader(UITools.GetHeader(), "$skin/styles/SN.Tagging.css");
        }

        /// <summary>
        /// Creates custom controls using the given content view.
        /// </summary>
        private void CreateControls()
        {
            try
            {
                var viewControl = Page.LoadControl(ContentViewPath) as Controls.TagAdminControl;
                if (viewControl != null)
                {
                    viewControl.TagPath = Tags;
                    viewControl.SearchPaths = SearchPaths.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    Controls.Add(viewControl);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }
    }
}
