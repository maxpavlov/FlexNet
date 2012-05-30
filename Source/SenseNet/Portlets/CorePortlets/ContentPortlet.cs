using System;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.PortletFramework;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{
    public class ContentPortlet : ContextBoundPortlet
    {
        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebDisplayName("Contentview path")]
        [WebDescription("Enter a custom Contentview path to use")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        [WebOrder(100)]
        public string ViewPath { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        public ContentPortlet()
        {
            this.Name = "Content viewer";
            this.Description = "This is a simple portlet for displaying a single content (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Content);

            Cacheable = true;   // by default, any contentportlet is cached
        }

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            if (this.RenderingMode == RenderMode.Ascx || this.RenderingMode == RenderMode.Native)
            {
                using (var traceOperation = Logger.TraceOperation("ContentPortlet.CreateChildControls", this.Name))
                {

                    Controls.Clear();

                    try
                    {
                        var node = GetContextNode();

                        if (node != null)
                        {
                            var content = Content.Create(node);
                            ContentView contentView = null;
                            if (!string.IsNullOrEmpty(ViewPath))
                            {
                                contentView = ContentView.Create(content, Page, ViewMode.Browse, ViewPath);
                            }
                            if (contentView == null)
                                contentView = ContentView.Create(content, Page, ViewMode.Browse);

                            Controls.Add(contentView);
                        }
                        else if (this.RenderException != null)
                        {
                            Controls.Clear();
                            Controls.Add(new System.Web.UI.WebControls.Label() { Text = string.Format("Error loading content view: {0}", this.RenderException.Message) });
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.WriteException(e);
                        Controls.Clear();
                        Controls.Add(new System.Web.UI.WebControls.Label() {Text = string.Format("Error loading content view: {0}", e.Message)});
                    }

                    ChildControlsCreated = true;
                    traceOperation.IsSuccessful = true;
                }
            }
        }

        protected override object GetModel()
        {
            var node = GetContextNode();
            return node == null ? null : Content.Create(node).GetXml(PortalActionLinkResolver.Instance);
        }
    }
}
