using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.PortletFramework;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{
    public class ContentPortlet : ContextBoundPortlet
    {
        [WebBrowsable(false)]
        [Personalizable(true)]
        //[LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        //[LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        //[WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        //[Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        //[ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView, PortletViewType.All)]
        //[WebOrder(100)]
        [Obsolete("Use Renderer property instead.")]
        public string ViewPath { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(true), Personalizable(true)]
        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView, PortletViewType.All)]
        [WebOrder(100)]
        public override string Renderer
        {
            get
            {
                return base.Renderer ?? ViewPath;
            }
            set
            {
                base.Renderer = value;
            }
        }

        public override RenderMode RenderingMode
        {
            get
            {
                if (string.IsNullOrEmpty(this.Renderer))
                    return RenderMode.Native;

                return this.Renderer.ToLower().EndsWith("xslt") ? RenderMode.Xslt : RenderMode.Ascx;
            }
            set
            {
                base.RenderingMode = value;
            }
        }

        public ContentPortlet()
        {
            this.Name = "Content viewer";
            this.Description = "This is a simple portlet for displaying a single content (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Content);

            Cacheable = true;   // by default, any contentportlet is cached
        }

        //=============================================================================== Overrides

        protected override void RenderWithAscx(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            if (ShowExecutionTime)
                Timer.Start();

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
                            if (!string.IsNullOrEmpty(Renderer))
                            {
                                contentView = ContentView.Create(content, Page, ViewMode.Browse, Renderer);
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
                        Controls.Add(new System.Web.UI.WebControls.Label() { Text = string.Format("Error loading content view: {0}", e.Message) });
                    }

                    ChildControlsCreated = true;
                    traceOperation.IsSuccessful = true;
                }
            }

            if (ShowExecutionTime)
                Timer.Stop();
        }

        protected override object GetModel()
        {
            var node = GetContextNode();
            return node == null ? null : Content.Create(node).GetXml(false);
        }
    }
}
