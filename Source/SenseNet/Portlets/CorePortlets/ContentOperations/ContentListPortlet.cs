using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI.ContentListViews;
using System.Web.UI;
using SenseNet.ContentRepository;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.ComponentModel;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Portal.Portlets
{
    public class ContentListPortlet : ContextBoundPortlet
    {

        private const string ResourceClassName = "ContentListPortlet";

        public ContentListPortlet()
        {
            this.Name = SenseNetResourceManager.Current.GetString(ResourceClassName, "PortletTitle");
            this.Description = SenseNetResourceManager.Current.GetString(ResourceClassName, "PortletDescription");
            this.Category = new PortletCategory(PortletCategoryType.Collection);

            this.HiddenProperties.Add("Renderer");
        }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.ContentList, EditorCategory.ContentList_Order)]
        [LocalizedWebDisplayName(ResourceClassName, "ViewFrameDisplayName"),
         LocalizedWebDescription(ResourceClassName, "ViewFrameDescription")]        
        [WebOrder(10)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ViewFrame)]
        public string ViewFrame { get; set; }

        [WebBrowsable(true)]
        [Personalizable(true)]
        [WebCategory(EditorCategory.ContentList, EditorCategory.ContentList_Order)]
        [LocalizedWebDisplayName(ResourceClassName, "DefaultViewDisplayName"),
         LocalizedWebDescription(ResourceClassName, "DefaultViewDescription")]        
        [WebOrder(20)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(PortletViewType.Ascx)]
        public string DefaultView { get; set; }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        protected override void CreateChildControls()
        {
            if (ShowExecutionTime)
                Timer.Start();

            Node ctx = null;

            try
            {
                ctx = ContextNode;
            }
            catch (SenseNetSecurityException)
            {
                //access denied to context node
            }

            if (ctx == null)
            {
                var l = new LiteralControl();
                if (!string.IsNullOrEmpty(RelativeContentPath))
                    l.Text = string.Format(SenseNetResourceManager.Current.GetString(ResourceClassName, "ContentDoesNotExist"), RelativeContentPath);
                else
                    l.Text = string.Format(SenseNetResourceManager.Current.GetString(ResourceClassName, "ContentListDoesNotExist"), RelativeContentPath);
                Controls.Add(l);

                if (ShowExecutionTime)
                    Timer.Stop();

                return;
            }

            if (Cacheable && CanCache && IsInCache)
            {
                if (ShowExecutionTime)
                    Timer.Stop();

                return;
            }

            if (RenderingMode == RenderMode.Native)
            {
                Controls.Clear();

                var c = CreateViewControl(ViewFrame);
                var frame = c as ViewFrame;
                if (frame != null)
                    frame.DefaultViewName = DefaultView;

                if (c is ViewFrame)
                    Controls.Add(c);
                else
                {
                    var l = new LiteralControl();
                    l.Text = string.Format(SenseNetResourceManager.Current.GetString(ResourceClassName, "ViewFrameIsNotValid"), RelativeContentPath);
                    Controls.Add(l);
                }

            }

            ChildControlsCreated = true;

            if (ShowExecutionTime)
                Timer.Stop();
        }

        private Control CreateViewControl(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                    return Page.LoadControl(path);
            }
            catch (Exception ex)
            {
                return new LiteralControl(string.Format(SenseNetResourceManager.Current.GetString(ResourceClassName, "ViewFramePathNotFound"), path, ex.Message));
            }
            return new Control();
        }
    }
}
