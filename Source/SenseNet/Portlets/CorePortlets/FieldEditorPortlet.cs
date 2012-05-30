using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.PortletFramework;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.Virtualization;
using System.Web;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.Portal.Portlets
{
    public class FieldEditorPortlet : ContextBoundPortlet, IContentProvider
    {
        public FieldEditorPortlet()
        {
            this.Name = "Field editor";
            this.Description = "This portlet edits the field setting with its edit contentview (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.System);
        }

        private string FieldName
        {
            get
            {
                var fn = HttpContext.Current.Request.Params["FieldName"];

                if (!string.IsNullOrEmpty(fn) && !fn.StartsWith("#"))
                    fn = string.Concat("#", fn);

                return fn;
            }
        }

        #region IContentProvider Members

        string IContentProvider.ContentTypeName
        {
            get;
            set;
        }

        string IContentProvider.ContentName
        {
            get
            {
                var ctn = FieldName;

                return string.IsNullOrEmpty(ctn) ? null : ctn;
            }
            set { }
        }

        #endregion

        protected override void CreateChildControls()
        {
            if (Cacheable && CanCache && IsInCache)
                return;

            Controls.Clear();

            var node = GetContextNode() as ContentList;
            if (node == null)
                return;

            var fieldName = FieldName;
            if (string.IsNullOrEmpty(fieldName))
                return;

            Content content = null;

            foreach (FieldSettingContent fieldSetting in node.FieldSettingContents)
            {
                if (fieldSetting.FieldSetting.Name.CompareTo(fieldName) != 0) 
                    continue;

                content = Content.Create(fieldSetting);
                content.Fields[FieldSetting.AddToDefaultViewName].FieldSetting.VisibleEdit = FieldVisibility.Hide;
                break;
            }

            if (content == null)
                return;

            var contentView = String.IsNullOrEmpty(this.Renderer) ?
                ContentView.Create(content, Page, ViewMode.InlineEdit) :
                ContentView.Create(content, Page, ViewMode.InlineEdit, this.Renderer);

            Controls.Add(contentView);

            ChildControlsCreated = true;
        }

        protected override void RenderWithAscx(System.Web.UI.HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }
    }
}
