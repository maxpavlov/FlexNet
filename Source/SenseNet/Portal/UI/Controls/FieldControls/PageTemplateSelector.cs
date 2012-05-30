using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using System.Globalization;

using SenseNet.ContentRepository;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:PageTemplateSelector ID=\"PageTemplateSelector1\" runat=server></{0}:PageTemplateSelector>")]
    public class PageTemplateSelector : FieldControl, INamingContainer, ITemplateFieldControl
    {
        // Members //////////////////////////////////////////////////////////////////////
        private static readonly string PreviewPicControlId = "previewPic";
        //private static readonly string PreviewPicHolderControlId = "sn-ctrl-previewpic";

        private static readonly string DefaultPreviewIconPath = RepositoryPath.Combine(Repository.PageTemplatesFolderPath, "pt_nopreview.png");
        private readonly ListBox _listControl;
        private string _previewPic = DefaultPreviewIconPath;
        
        // Constructor //////////////////////////////////////////////////////////////////
        public PageTemplateSelector()
        {
            InputUnitCssClass = "sn-inputunit sn-iu-previewchooser";
            _listControl = new ListBox
                               {
                                   AutoPostBack = false, 
                                   SelectionMode = ListSelectionMode.Single, 
                                   Rows = 10
                               };
            _listControl.Attributes.Add("onchange", GetOnChangeScript(PreviewPicControlId));
            LoadPageTemplates(_listControl);
        }

        // Methods ////////////////////////////////////////////////////////////////
        public override void SetData(object data)
        {
            var dataNode = data as Node;
            if (dataNode == null) return;
            
            SetSelectedPageTemplate(dataNode as PageTemplate, _listControl);

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as ListBox;
            var img = GetPreviewPicControl() as System.Web.UI.WebControls.Image;
            if (title != null) title.Text = Field.DisplayName;
            if (desc != null) desc.Text = Field.Description;
            
            if (innerControl == null) return;
            if (img != null)
                innerControl.Attributes.Add("onchange", GetOnChangeScript(img.ClientID));
            LoadPageTemplates(innerControl);
            SetSelectedPageTemplate(dataNode as PageTemplate, innerControl);
            
            #endregion

        }

        public override object GetData()
        {
            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return _listControl.SelectedIndex < 0 ? null : GetPageTemplateNode(_listControl);

            var innerControl = GetInnerControl() as ListBox;
            if (innerControl != null)
                return innerControl.SelectedIndex < 0 ? null : GetPageTemplateNode(innerControl);
            
            return _listControl.SelectedIndex < 0 ? null : GetPageTemplateNode(_listControl);
        }

        // Events /////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;

            _listControl.CssClass = String.IsNullOrEmpty(CssClass) ? "sn-ctrl-select" : CssClass;
            Controls.Add(_listControl);
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {

            #region template

            if (UseBrowseTemplate)
            {
                ProcessImage();
                base.RenderContents(writer);
                return;
            }
            if (UseEditTemplate)
            {
                ProcessImage();
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }
            if (UseInlineEditTemplate)
            {
                ProcessImage();
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }

            #endregion


			if (RenderMode == FieldControlRenderMode.Browse)
				RenderSimple(writer);
			else
				RenderEditor(writer);
		}

        private void ProcessImage()
        {
            var img = GetPreviewPicControl() as System.Web.UI.WebControls.Image;
            if (img == null) return;
            img.GenerateEmptyAlternateText = true;
            img.ImageUrl = _previewPic;
        }

        private void ManipulateTemplateControls()
        {
            var ic = GetInnerControl() as ListBox;
            if (ic == null) return;
            if (!Field.ReadOnly && !ReadOnly) return;
            ic.Enabled = false;
            ic.EnableViewState = false;
        }
		private void RenderSimple(TextWriter writer) { if(_listControl.SelectedItem != null) writer.Write(_listControl.SelectedItem.Text); }
		private void RenderEditor(HtmlTextWriter writer)
		{
			if (this.RenderMode == FieldControlRenderMode.InlineEdit)
            {
                var titleText = String.Concat(this.Field.DisplayName, " ", this.Field.Description);
                _listControl.Attributes.Add("Title", titleText);
            }

            if (this.Field.ReadOnly)
            {
                _listControl.Enabled = false;
                _listControl.EnableViewState = false;
                _listControl.RenderControl(writer);
            }
            else if (this.ReadOnly)
            {
                _listControl.Enabled = !this.ReadOnly;
                _listControl.EnableViewState = false;
                _listControl.RenderControl(writer);
            }
            else
                _listControl.RenderControl(writer);
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                base.RenderEndTag(writer);
                return;
            }
            RenderPreviewHtmlFragment(writer);
            base.RenderEndTag(writer);
        }

        
        // Internals //////////////////////////////////////////////////////////////
        private void SetSelectedPageTemplate(PageTemplate pageTemplate, ListBox listControl)
        {
            if (pageTemplate == null) 
                throw new ArgumentNullException("pageTemplate");
            if (listControl == null) 
                throw new ArgumentNullException("listControl");

            var selectedIndex = listControl.Items.IndexOf(listControl.Items.FindByValue(pageTemplate.Name));
            listControl.SelectedIndex = selectedIndex;

            GetPreviewPicture(pageTemplate);
        }
        private void GetPreviewPicture(PageTemplate pageTemplate)
        {
            if (pageTemplate == null) throw new ArgumentNullException("pageTemplate");
            _previewPic = (!ExistsPreviewIcon(pageTemplate)) ? DefaultPreviewIconPath : RepositoryPath.Combine(Repository.PageTemplatesFolderPath, string.Concat(pageTemplate.Name, ".png"));
        }
        private void RenderPreviewHtmlFragment(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-ctrl-previewpic");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "previewPic");
            writer.AddAttribute(HtmlTextWriterAttribute.Src, _previewPic);
            writer.AddAttribute(HtmlTextWriterAttribute.Alt, string.Empty);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
            writer.RenderEndTag();  //</div>
        }

        private static void LoadPageTemplates(ListBox listBox)
        {
            if (listBox == null) 
                throw new ArgumentNullException("listBox");
            
            var pageTemplates = GetPageTemplates();

            if (pageTemplates.Count == 0)
                throw new ApplicationException(String.Format(CultureInfo.InvariantCulture, "Couldn't find any pagetemplates."));

            AddListToControl(pageTemplates.Nodes, listBox);
        }
        private static void AddListToControl(IEnumerable<Node> pageTemplates, ListBox listBox)
        {
            if (listBox == null) 
                throw new ArgumentNullException("listBox");
            
            foreach (PageTemplate item in pageTemplates)
            {
                var listItem = new ListItem { Text = item.Name, Value = item.Name };
                if (!ExistsPreviewIcon(item)) listItem.Attributes.Add("class", "nopreview");
                listBox.Items.Add(listItem);
            }
        }
        private static bool ExistsPreviewIcon(PageTemplate pageTemplate)
        {
            if (pageTemplate == null) throw new ArgumentNullException("pageTemplate");
            var path = String.Concat(pageTemplate.Path, ".png");
            var node = Node.LoadNode(path);
            return (node == null ? false : true);
        }
        private static string GetOnChangeScript(string pictureElementId)
        {
            return string.Format("this.options[this.selectedIndex].className=='nopreview'?document.getElementById('{2}').src='{0}':document.getElementById('{2}').src='{1}/'+this.value+'.png'", DefaultPreviewIconPath, Repository.PageTemplatesFolderPath,pictureElementId);
        }
        private static NodeQueryResult GetPageTemplates()
        {
            var query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes[typeof(PageTemplate).Name], true));
            query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, Repository.PageTemplatesFolderPath));
            query.Orders.Add(new SearchOrder(StringAttribute.Name, OrderDirection.Asc));
            return query.Execute();
        }
        private static Node GetPageTemplateNode(ListControl listControl)
        {
            if (listControl == null) throw new ArgumentNullException("listControl");
            return Node.LoadNode(RepositoryPath.Combine(Repository.PageTemplatesFolderPath, listControl.SelectedItem.Text));
        }

        #region ITemplateFieldControl Members
        
        public Control GetInnerControl() { return this.FindControlRecursive(InnerControlID); }
        public Control GetLabelForDescription() { return this.FindControlRecursive(DescriptionControlID); }
        public Control GetLabelForTitleControl() { return this.FindControlRecursive(TitleControlID); }
        
        #endregion

        public Control GetPreviewPicControl() { return this.FindControlRecursive(PreviewPicControlId); }
    }
}