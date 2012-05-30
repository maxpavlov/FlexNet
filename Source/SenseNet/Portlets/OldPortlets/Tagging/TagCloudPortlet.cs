using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using SenseNet.Portal.Portlets.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.UI;


namespace SenseNet.Portal.Portlets
{
    public class TagCloudPortlet : ContextBoundPortlet
    {
        private string contentViewPath = "/Root/System/SystemPlugins/Portlets/TagCloud/TagCloudPortlet.ascx";
        private string searchPaths = "";
        private string contentTypes = "";
        private string searchPortletPath = "";



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

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Tag search portlet path")]
        [WebDescription("Path of the portlet used for search")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        public string SearchPortletPath
        {
            get { return searchPortletPath; }
            set { searchPortletPath = value; }

        }

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

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Max tag count (TOP)")]
        [WebDescription("Tha maximum number of different tags to display in the cloud based on occurrencies")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(60)]
        public int MaxCount { get; set; }


        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Content Types")]
        [WebDescription("Only these content types will be examined for tags (separate with commas)")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(AllowedContentTypes = "ContentType;Folder")]
        [WebOrder(120)]
        public string ContentTypes
        {
            get { return contentTypes; }
            set { contentTypes = value; }

        }

        [WebBrowsable(true), Personalizable(true), WebDisplayName("Including descendant types"), WebDescription("the listed content types and descendants will be examined"), WebCategory(EditorCategory.Search, EditorCategory.Search_Order), WebOrder(130)]
        public bool IncludeChildren { get; set; }


        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        public TagCloudPortlet()
        {
            IncludeChildren = false;
            Name = "Tag cloud";
            Description = "Portlet for rendering a tag cloud";
            Category = new PortletCategory(PortletCategoryType.Application);
        }


        protected override void OnPreRender(EventArgs e)
        {
            UITools.AddStyleSheetToHeader(UITools.GetHeader(), "$skin/styles/SN.Tagging.css");
            using (var traceOperation = Logger.TraceOperation("TagCloudPortlet.OnPreRender: ", this.Name))
            {
                base.OnPreRender(e);
                //if cached return
                if (Cacheable && CanCache && IsInCache)
                {
                    System.Diagnostics.Trace.WriteLine(this.ID + " OnPreRender END - Portlet cached.");
                    return;
                }

                char[] splitchars = { ',' };
                var contentTypeArray = ContentTypes.Split(splitchars, StringSplitOptions.RemoveEmptyEntries);
                var searchPathArray = !string.IsNullOrEmpty(SearchPaths)
                                          ? SearchPaths.ToLower().Split(splitchars, StringSplitOptions.RemoveEmptyEntries)
                                          : (ContextNode == null ? new string[0] : new[] {ContextNode.Path.ToLower()});
                var allTypes = new List<string>();

                foreach (var type in contentTypeArray)
                {
                    allTypes.Add(type.ToLower());

                    if (IncludeChildren) //if all descendant types are needed
                    {
                        var baseType = ContentType.GetByName(type);
                        if (baseType != null)
                        {
                            foreach (var childType in ContentType.GetContentTypes())
                            {
                                if (childType.IsDescendantOf(baseType))
                                {
                                    allTypes.Add(childType.Name.ToLower());
                                }
                            }
                        }
                    }
                }

                var ctrl = Page.LoadControl(contentViewPath) as TagCloudControl;
                if (ctrl != null)
                {
                    var repeater = ctrl.FindControl("TagCloudRepeater") as System.Web.UI.WebControls.Repeater;
                    if (repeater != null)
                    {
                        if (allTypes.Count == 0 && searchPathArray.Count() == 0)
                        {
                            repeater.DataSource = TagManager.GetTagClasses(MaxCount);
                        }
                        else
                        {
                            repeater.DataSource = TagManager.GetTagClasses(searchPathArray, allTypes.ToArray(), MaxCount);
                        }
                        ctrl.SearchPortletPath = SearchPortletPath;
                        repeater.DataBind();
                    }

                    Controls.Add(ctrl);
                }
                else
                {
                    Controls.Add(new LiteralControl("ContentView error."));
                }
                traceOperation.IsSuccessful = true;
            }
        }

    }
}