using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using SenseNet.ContentRepository.Fields;
using SnFields = SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.UI.Controls
{
	[ToolboxData("<{0}:HyperLink ID=\"HyperLink1\" runat=server></{0}:HyperLink>")]
	public class HyperLink : FieldControl, INamingContainer, ITemplateFieldControl
	{
        // Members //////////////////////////////////////////////////////////////////////
		private SnFields.HyperLinkField.HyperlinkData _data; //-- if simple

        private readonly string LinkTextTextBoxId = "_text_";
        private readonly TextBox _linkTextTextBox; // -- if editor
        private readonly string LinkHrefTextBoxId = "_href_";
        private readonly TextBox _linkHrefTextBox;
        private readonly string LinkTargetTextBoxId = "_target_";
        private readonly TextBox _linkTargetTextBox;
        private readonly string LinkTitleTextBoxId = "_title_";
		private readonly TextBox _linkTitleTextBox;

        private readonly string LabelTextControlId = "TextLabel";
	    private readonly string LabelHrefControlId = "HrefLabel";
        private readonly string LabelTargetControlId = "TargetLabel";
        private readonly string LabelLinkControlId = "LinkLabel";
        
		private string _textLabel = "Link Text";
		private string _hrefLabel = "Link Href";
		private string _targetLabel = "Link Target";
        private string _titleLabel = "Link Title";

	    #region properties

	    [PersistenceMode(PersistenceMode.Attribute)]
	    public string TitleLabel
	    {
	        get { return _titleLabel; }
	        set { _titleLabel = value; }
	    }
	    [PersistenceMode(PersistenceMode.Attribute)]
	    public string HrefLabel
	    {
	        get { return _hrefLabel; }
	        set { _hrefLabel = value; }
	    }
	    [PersistenceMode(PersistenceMode.Attribute)]
	    public string TargetLabel
	    {
	        get { return _targetLabel; }
	        set { _targetLabel = value; }
	    }
	    [PersistenceMode(PersistenceMode.Attribute)]
	    public string TextLabel
	    {
	        get { return _titleLabel; }
	        set { _titleLabel = value; }
	    }

	    #endregion
        
        // Constructor //////////////////////////////////////////////////////////////////
		public HyperLink()
		{
			_linkTextTextBox = new TextBox { ID = LinkTextTextBoxId };
			_linkHrefTextBox = new TextBox { ID = LinkHrefTextBoxId };
			_linkTargetTextBox = new TextBox { ID = LinkTargetTextBoxId };
			_linkTitleTextBox = new TextBox { ID = LinkTitleTextBoxId };
		}

		public override void SetData(object data)
		{
			_data = (SnFields.HyperLinkField.HyperlinkData)data;

            _linkTextTextBox.Text = _data.Text;
            _linkHrefTextBox.Text = _data.Href;
            _linkTargetTextBox.Text = _data.Target;
            _linkTitleTextBox.Text = _data.Title;

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;
            //
            // innercontrol disabled 'cause there are 4 other contorls.
            //
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var linkText = GetLinkTextTextBox() as TextBox;
            var linkHref = GetLinkHrefTextBox() as TextBox;
            var linkTarget = GetLinkTargetTextBox() as TextBox;
            var linkTitle = GetLinkTitleTextBox() as TextBox;

            if (title != null) title.Text = Field.DisplayName;
            if (desc != null) desc.Text = Field.Description;
            if (linkText != null) linkText.Text = _data.Text;
            if (linkHref != null) linkHref.Text = _data.Href;
            if (linkTarget != null) linkTarget.Text = _data.Target;
            if (linkTitle != null) linkTitle.Text = _data.Title;

            

		}
		public override object GetData()
		{
            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
            {
                return new SnFields.HyperLinkField.HyperlinkData(
                    _linkHrefTextBox.Text,
                    _linkTextTextBox.Text,
                    _linkTitleTextBox.Text,
                    _linkTargetTextBox.Text);               
            }
            var linkText = GetLinkTextTextBox() as TextBox;
            var linkHref = GetLinkHrefTextBox() as TextBox;
            var linkTarget = GetLinkTargetTextBox() as TextBox;
            var linkTitle = GetLinkTitleTextBox() as TextBox;

		    return new SnFields.HyperLinkField.HyperlinkData(
                (linkHref != null ? linkHref.Text : _linkHrefTextBox.Text),
                (linkText != null ? linkText.Text : _linkTextTextBox.Text),
                (linkTitle != null ? linkTitle.Text : _linkTitleTextBox.Text),
                (linkTarget != null ? linkTarget.Text : _linkTargetTextBox.Text));

		}
        public override object Data
        {
            get
            {
                var data = (SnFields.HyperLinkField.HyperlinkData)GetData();
                data.Text = SenseNet.Portal.Security.Sanitize(data.Text);
                return data;
            }
        }

		protected override void OnInit(EventArgs e)
		{
            base.OnInit(e);

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;
            
            _linkHrefTextBox.Width = this.Width;
			_linkHrefTextBox.CssClass = "sn-ctrl sn-ctrl-text";

			_linkTextTextBox.Width = this.Width;
			_linkTextTextBox.CssClass = "sn-ctrl sn-ctrl-text";

			_linkTitleTextBox.Width = this.Width;
			_linkTitleTextBox.CssClass = "sn-ctrl sn-ctrl-text";

			_linkTargetTextBox.Width = this.Width;
			_linkTargetTextBox.CssClass = "sn-ctrl sn-ctrl-text";

            Controls.Add(_linkHrefTextBox);
            Controls.Add(_linkTextTextBox);
            Controls.Add(_linkTitleTextBox);
            Controls.Add(_linkTargetTextBox);
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
            if (UseBrowseTemplate)
            {
                base.RenderContents(writer);
                return;
            }
            if (UseEditTemplate)
            {
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }
            if (UseInlineEditTemplate)
            {
                ManipulateTemplateControls();
                base.RenderContents(writer);
                return;
            }

			if (RenderMode == FieldControlRenderMode.Browse)
				RenderSimple(writer);
			else
				RenderEditor(writer);
		}

        private void ManipulateTemplateControls()
        {
            var linkHrefTextBox = GetLinkHrefTextBox() as TextBox;
            var linkTextTextBox = GetLinkTextTextBox() as TextBox;
            var linkTitleTextBox = GetLinkTitleTextBox() as TextBox;
            var linkTargetTextBox = GetLinkTargetTextBox() as TextBox;

            if (Field.ReadOnly)
            {
                if (linkHrefTextBox != null) linkHrefTextBox.Enabled = false;
                if (linkTextTextBox != null) linkTextTextBox.Enabled = false;
                if (linkTitleTextBox != null) linkTitleTextBox.Enabled = false;
                if (linkTargetTextBox != null) linkTargetTextBox.Enabled = false;
            }

            var textLabel = GetTextLabel() as Label;
            var hrefLabel = GetHrefLabel() as Label;
            var targetLabel = GetTargetLabel() as Label;
            var linkLabel = GetLinkLabel() as Label;
            
            if (textLabel != null) textLabel.Text = _textLabel;
            if (hrefLabel != null) hrefLabel.Text = _hrefLabel;
            if (targetLabel != null) targetLabel.Text = _targetLabel;
            if (linkLabel != null) linkLabel.Text = _titleLabel;


        }
		private void RenderEditor(HtmlTextWriter writer)
		{

			if (Field.ReadOnly)
			{
				_linkHrefTextBox.Enabled = false;
				_linkTextTextBox.Enabled = false;
				_linkTitleTextBox.Enabled = false;
				_linkTargetTextBox.Enabled = false;
			}

			writer.AddAttribute(HtmlTextWriterAttribute.For, _linkTextTextBox.ClientID);
			writer.RenderBeginTag(HtmlTextWriterTag.Label);
			writer.Write(this._textLabel);
			writer.RenderEndTag();
			writer.WriteBreak();
			_linkTextTextBox.RenderControl(writer);
			writer.WriteBreak();

			writer.AddAttribute(HtmlTextWriterAttribute.For, _linkHrefTextBox.ClientID);
			writer.RenderBeginTag(HtmlTextWriterTag.Label);
			writer.Write(this._hrefLabel);
			writer.RenderEndTag();
			writer.WriteBreak();
			_linkHrefTextBox.RenderControl(writer);
			writer.WriteBreak();

			writer.AddAttribute(HtmlTextWriterAttribute.For, _linkTitleTextBox.ClientID);
			writer.RenderBeginTag(HtmlTextWriterTag.Label);
			writer.Write(this._titleLabel);
			writer.RenderEndTag();
			writer.WriteBreak();
			_linkTitleTextBox.RenderControl(writer);
			writer.WriteBreak();

			writer.AddAttribute(HtmlTextWriterAttribute.For, _linkTargetTextBox.ClientID);
			writer.RenderBeginTag(HtmlTextWriterTag.Label);
			writer.Write(this._targetLabel);
			writer.RenderEndTag();
			writer.WriteBreak();
			_linkTargetTextBox.RenderControl(writer);
		}
		private void RenderSimple(HtmlTextWriter writer)
		{
			writer.Write("<a");
			if (!String.IsNullOrEmpty(_data.Href))
				WriteAttribute("href", _data.Href, writer);
			if (!String.IsNullOrEmpty(_data.Title))
				WriteAttribute("title", _data.Title, writer);
			if (!String.IsNullOrEmpty(_data.Target))
				WriteAttribute("target", _data.Target, writer);
			writer.Write(">");
			if (!String.IsNullOrEmpty(_data.Text))
				writer.Write(_data.Text);
			writer.Write("</a>");
		}
		private void WriteAttribute(string name, string value, HtmlTextWriter writer)
		{
			writer.Write(" ");
			writer.Write(name);
			writer.Write("=\"");
			writer.Write(value);
			writer.Write("\"");
		}

        #region ITemplateFieldControl Members
        
        public Control GetInnerControl() { return null; } // disabled
        public Control GetLabelForDescription() { return this.FindControlRecursive(DescriptionControlID); }
        public Control GetLabelForTitleControl() { return this.FindControlRecursive(TitleControlID); }
        
        #endregion

        public Control GetLinkTextTextBox() { return this.FindControlRecursive(LinkTextTextBoxId); }
        public Control GetLinkHrefTextBox() { return this.FindControlRecursive(LinkHrefTextBoxId); }
        public Control GetLinkTargetTextBox() { return this.FindControlRecursive(LinkTargetTextBoxId); }
        public Control GetLinkTitleTextBox() { return this.FindControlRecursive(LinkTitleTextBoxId); }
        public Control GetTextLabel() { return this.FindControlRecursive(LabelTextControlId); }
        public Control GetHrefLabel() { return this.FindControlRecursive(LabelHrefControlId); }
        public Control GetTargetLabel() { return this.FindControlRecursive(LabelTargetControlId); }
        public Control GetLinkLabel() { return this.FindControlRecursive(LabelLinkControlId); }

    }
}