using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class ContentLinkerPortlet : ContentCollectionPortlet
    {
        private string _viewPath = "/Root/System/SystemPlugins/Portlets/ContentLinker/ContentLinker.ascx";

        public ContentLinkerPortlet()
        {
            this.Name = "Linker";
            this.Description = "A portlet for creating content links (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Collection);

            this.HiddenProperties.Add("Renderer");

            Cacheable = false;   // by default, caching is switched off
            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Cache };
        }

        //================================================================ Portlet properties

        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPath
        {
            get { return _viewPath; }
            set { _viewPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        //================================================================ Controls

        private Button _linkerButton;
        protected Button LinkerButton
        {
            get
            {
                return _linkerButton ?? (_linkerButton = this.FindControlRecursive("LinkerButton") as Button);
            }
        }

        private Label _contentLabel;
        protected Label ContentLabel
        {
            get { return _contentLabel ?? (_contentLabel = this.FindControlRecursive("ContentName") as Label); }
        }

        //================================================================ Methods

        protected override void CreateChildControls()
        {
            Controls.Clear();

            try
            {
                if (string.IsNullOrEmpty(ViewPath))
                    return;

                var viewControl = Page.LoadControl(ViewPath) as UserControl;
                if (viewControl == null)
                    return;

                Controls.Add(viewControl);

                var genericContent = GetContextNode() as GenericContent;
                if (genericContent == null)
                    return;

                if (ContentLabel != null)
                    ContentLabel.Text = genericContent.DisplayName;

                BindEvents();
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            ChildControlsCreated = true;
        }

        protected void BindEvents()
        {
            if (LinkerButton == null) 
                return;

            LinkerButton.Click += LinkContentsButton_Click;
            LinkerButton.Visible = RequestIdList.Count > 0;
        }

        //================================================================ Events

        protected void LinkContentsButton_Click(object sender, EventArgs e)
        {
            var targetNode = GetContextNode();
            
            if (targetNode != null)
            {
                foreach (var node in this.RequestNodeList)
                {
                    var link = new ContentLink(targetNode);
                    link.Link = node;
                    link.Save();
                }
            }

            CallDone();
        }
    }
}
