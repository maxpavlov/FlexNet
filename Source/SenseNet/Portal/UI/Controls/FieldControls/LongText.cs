using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:LongText ID=\"LongText1\" runat=server></{0}:LongText>")]
    public class LongText : FieldControl, INamingContainer, ITemplateFieldControl
    {
        private readonly TextBox _inputTextBox;

        // Properties ///////////////////////////////////////////////////////////////////
        [PersistenceMode(PersistenceMode.Attribute)]
        public int MaxLength { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public int Rows { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool FullScreenText { get; set; }

        // Constructor //////////////////////////////////////////////////////////////////
        public LongText()
        {
            _inputTextBox = new TextBox();
        }

        // Methods //////////////////////////////////////////////////////////////////////
        public override void SetData(object data)
        {
            var t = (string) data;
            _inputTextBox.Text = t;

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerCtl = GetInnerControl() as TextBox;
            if (title != null) title.Text = Field.DisplayName;
            if (desc != null) desc.Text = Field.Description;
            if (innerCtl != null) innerCtl.Text = t;
        }

        public override object GetData()
		{
            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return _inputTextBox.Text;
            var innerCtl = GetInnerControl() as TextBox;
            return innerCtl != null ? innerCtl.Text : _inputTextBox.Text;
        }

        // Events ///////////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            UITools.AddScript(UITools.ClientScriptConfigurations.SNBinaryFieldControlPath);

            base.OnInit(e);

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                if (this.FullScreenText)
                {
                    var textBox = GetInnerControl() as TextBox;
                    textBox.CssClass = string.Concat(textBox.CssClass, " sn-highlighteditor");
                }
                return;
            }

            _inputTextBox.CssClass = String.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-text" : this.CssClass;
            _inputTextBox.MaxLength = MaxLength;
            _inputTextBox.Rows = Rows;
            _inputTextBox.TextMode = TextBoxMode.MultiLine;

            if (this.FullScreenText)
            {
                _inputTextBox.CssClass = string.Concat(_inputTextBox.CssClass, " sn-highlighteditor");
            }

            Controls.Add(_inputTextBox);
            //base.OnInit(e);
            //_inputTextBox.ID = String.Concat(this.ID, "_", this.ContentHandler.Id.ToString());
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

			if (this.RenderMode == FieldControlRenderMode.Browse)
				RenderSimple(writer);
			else
				RenderEditor(writer);
		}

        private void ManipulateTemplateControls()
        {
            //
            //  This method is needed to ensure the common fieldcontrol logic.
            //
            var innerShortText = GetInnerControl() as TextBox;
            var lt = GetLabelForTitleControl() as Label;
            var ld = GetLabelForDescription() as Label;

            if (innerShortText == null) return;

            if (Field.ReadOnly)
            {
                var p = innerShortText.Parent;
                if (p != null)
                {
                    p.Controls.Remove(innerShortText);
                    if (lt != null) lt.AssociatedControlID = string.Empty;
                    if (ld != null) ld.AssociatedControlID = string.Empty;
                    p.Controls.Add(new LiteralControl(innerShortText.Text));
                }
            }
            else if (ReadOnly)
            {
                innerShortText.Enabled = !ReadOnly;
                innerShortText.EnableViewState = false;
            }

        }
		private void RenderSimple(HtmlTextWriter writer)
		{
			writer.Write(_inputTextBox.Text);
		}
		private void RenderEditor(HtmlTextWriter writer)
		{
			if (this.RenderMode == FieldControlRenderMode.InlineEdit)
            {
                var titleText = String.Concat(this.Field.DisplayName, " ", this.Field.Description);
                _inputTextBox.Attributes.Add("Title", titleText);
            }

            if (this.Field.ReadOnly)
            {
                writer.Write(_inputTextBox.Text);
            }
            else if (this.ReadOnly)
            {
                _inputTextBox.Enabled = !this.ReadOnly;
                _inputTextBox.EnableViewState = false;
                _inputTextBox.RenderControl(writer);
            }
            else
            {
                // render read/write control
                _inputTextBox.RenderControl(writer);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            if (this.FullScreenText)
                UITools.RegisterStartupScript("inithighlight", "SN.BinaryFieldControl.initHighlightTextbox('xml');", this.Page);
        }

        #region ITemplateFieldControl Members

        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID) as TextBox;
        }
        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID) as Label;
        }
        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID) as Label;
        }

        #endregion
    }
}