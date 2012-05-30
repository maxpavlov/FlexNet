using System;
using System.ComponentModel;
using System.Linq;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Diagnostics;
using System.Web.UI;
using SenseNet.Portal.Portlets.Controls;
using SenseNet.ContentRepository.Storage;
using System.Collections;
using System.Collections.Generic;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.Portal.Portlets
{
    public class TagSearchPortlet : ContextBoundPortlet
    {
        private string contentViewPath = "/Root/System/SystemPlugins/Portlets/TagSearch/TagSearch.ascx";
        private string searchPaths = "";
        private string contentTypes = "";

        public delegate string TagSearchHandler(EventArgs e, string tag);

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

        [WebDisplayName("Children filter")]
        [WebDescription("Optional filter for the children query")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Collection, EditorCategory.Collection_Order), WebOrder(50)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MiddleSize)]
        public string QueryFilter { get; set; }

        /// <summary>
        /// Initalize the portlet name and description
        /// </summary>
        public TagSearchPortlet()
        {
            IncludeChildren = false;
            Name = "Tag search";
            Description = "Portlet for searching by tag (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Application);
        }

        /// <summary>
        /// Load the content view if possible, and get the attributes for it.
        /// </summary>
        private void CreateControls()
        {
            try
            {
                var viewControl = Page.LoadControl(ContentViewPath) as TagSearch;
                if (viewControl != null && ContextNode != null)
                {
                    viewControl.Results = GetResult(ContextNode);
                    viewControl.LastSearchedTag = GetSearchedTag(ContextNode);
                    viewControl.TagSearching += viewControl_TagSearch;
                    Controls.Add(viewControl);
                }
                else if (this.RenderException != null)
                {
                    Controls.Add(new LiteralControl(string.Concat("Portlet error: " + this.RenderException.Message)));
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }

        /// <summary>
        /// Handle that clcik onthe button in TagSearch portlet.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <param name="tag">Value of the serched tag</param>
        /// <returns>Return the new URL which has the TagFilter prameter for searchig</returns>
        string viewControl_TagSearch(EventArgs e, string tag)
        {
            string newUrl;

            if (Context.Request.Params.Get("TagFilter") == null)
            {
                var reqUrl = SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri.ToString();
                newUrl = reqUrl.Contains('?') ?
                    (string.Format("{0}&TagFilter={1}", reqUrl, tag)) : (string.Format("{0}?TagFilter={1}", reqUrl, tag));
            }
            else
            {
                var tempUrl = SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri.ToString();
                var tempFilter = Context.Request.Params.Get("TagFilter");
                newUrl = tempUrl.Replace("TagFilter=" + tempFilter, "TagFilter=" + tag);
            }

            
            return newUrl;
            
        }

        /// <summary>
        /// Get the tag name which we're searching 
        /// </summary>
        /// <param name="actNode">The actual Node</param>
        /// <returns>Return a string value which is the searched tag name</returns>
        private string GetSearchedTag(Node actNode)
        {
            var searchedTag = string.Empty;

            if (Context.Request.Params.Get("TagFilter") != null)
                searchedTag = Context.Request.Params.Get("TagFilter");
            
            if (actNode.NodeType.Id == ActiveSchema.NodeTypes["Tag"].Id)
                searchedTag = actNode.Name;
            
            return searchedTag;
        }

        /// <summary>
        /// Get the search result
        /// </summary>
        /// <param name="actNode">The actual Node</param>
        /// <returns>Return IENumerable value which is te search value</returns>
        private IEnumerable GetResult(Node actNode)
        {
            var queryString = string.Empty;

            if (Context.Request.Params.Get("TagFilter") != null)
                queryString = Context.Request.Params.Get("TagFilter");

            if (actNode.NodeType.Id == ActiveSchema.NodeTypes["Tag"].Id)
                queryString = actNode.Name;

            char[] splitchars = { ',' };
            var contentTypeArray = ContentTypes.Split(splitchars, StringSplitOptions.RemoveEmptyEntries);
            var searchPathArray = SearchPaths.ToLower().Split(splitchars, StringSplitOptions.RemoveEmptyEntries);
            var allTypes = new List<string>();

            foreach (var type in contentTypeArray)
            {
                allTypes.Add(type.ToLower());

                if (IncludeChildren)//if all descendant types are needed
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

            var contentsId = TagManager.GetNodeIds(queryString, searchPathArray, allTypes.ToArray(), QueryFilter);
            
            var result = new NodeList<Node>(contentsId);

            return result;
        }

        /// <summary>
        /// Owerrided method, initalize the portlet
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            CreateControls();

        }

    }
}
