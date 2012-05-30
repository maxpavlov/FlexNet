using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:ShortText ID=\"ShortText1\" runat=server></{0}:ShortText>")]
    public class ShortText : FieldControl, INamingContainer, ITemplateFieldControl
    {
        // Fields ///////////////////////////////////////////////////////////////////////
        private string _text;
        protected readonly TextBox _shortTextBox;
        [PersistenceMode(PersistenceMode.Attribute)]
        public int MaxLength { get; set; }

        // Constructor //////////////////////////////////////////////////////////////////
        public ShortText()
        {
            InnerControlID = "InnerShortText";
            _shortTextBox = new TextBox { ID = InnerControlID };
        }

        // Methods //////////////////////////////////////////////////////////////////////
        public override void SetData(object data)
        {
            if (data == null)
                _text = null;
            else
                _text = data.GetType() == typeof(System.Drawing.Color) ? ColorField.ColorToString((System.Drawing.Color)data) : data.ToString();

            _shortTextBox.Text = Convert.ToString(_text);

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            // synchronize data with controls are given in the template
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as TextBox;
            if (title != null)
                title.Text = this.Field.DisplayName;
            if (desc != null)
                desc.Text = this.Field.Description;
            if (innerControl != null)
                innerControl.Text = Convert.ToString(_text);

            #endregion

        }
        public override object GetData()
        {
            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return _shortTextBox.Text;
            var innerControl = GetInnerControl() as TextBox;
            return innerControl != null ? innerControl.Text : _shortTextBox.Text;

            #endregion
        }

        // Events ///////////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            #region template

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;

            #endregion

            #region original flow

            _shortTextBox.Visible = false;
            _shortTextBox.Width = this.Width;
            _shortTextBox.MaxLength = this.MaxLength;
            _shortTextBox.CssClass = string.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-text" : this.CssClass;
            Controls.Add(_shortTextBox);

            #endregion
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            #region template

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

            #endregion

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
                if (p!= null)
                {
                    p.Controls.Remove(innerShortText);
                    if (lt != null) lt.AssociatedControlID = string.Empty;
                    if (ld != null) ld.AssociatedControlID = string.Empty;
                    p.Controls.Add(new LiteralControl(innerShortText.Text));
                }
            } else if (ReadOnly)
            {
                innerShortText.Enabled = !ReadOnly;
                innerShortText.EnableViewState = false;                        
            }
        }

        #region ITemplateFieldControl members

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

        #region backward compatibility

        private void RenderSimple(TextWriter writer)
        {
            writer.Write(_text);
        }
        private void RenderEditor(HtmlTextWriter writer)
        {
            _shortTextBox.Visible = true;
            if (this.RenderMode == FieldControlRenderMode.InlineEdit)
            {
                var altText = String.Concat(this.Field.DisplayName, " ", this.Field.Description);
                _shortTextBox.Attributes.Add("Title", altText);
            }

            if (this.Field.ReadOnly)
            {
                // label
                writer.Write(_shortTextBox.Text);
            }
            else if (this.ReadOnly)
            {
                // render readonly control
                _shortTextBox.Enabled = !this.ReadOnly;
                _shortTextBox.EnableViewState = false;
                _shortTextBox.RenderControl(writer);
            }
            else
            {
                // render read/write control
                _shortTextBox.RenderControl(writer);
            }
        }

        #endregion
    }
}