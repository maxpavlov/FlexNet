using System;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:Number ID=\"Number1\" runat=server></{0}:Number>")]
    public class Number : FieldControl, INamingContainer, ITemplateFieldControl
    {
        // Properties ///////////////////////////////////////////////////////////////////
        private readonly TextBox _inputTextBox;
        // Constructor //////////////////////////////////////////////////////////////////
		public Number() { _inputTextBox = new TextBox { ID = InnerControlID }; }
        // Methods //////////////////////////////////////////////////////////////////////
        public override object GetData()
        {
            var stringValue = _inputTextBox.Text;

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                var innerControl = GetInnerControl() as TextBox;
                if (innerControl != null)
                    stringValue = innerControl.Text;
            }

            return stringValue.Length == 0 ? default(decimal) : Convert.ToDecimal(stringValue);
        }
        public override void SetData(object data)
        {
            var templated = UseBrowseTemplate || UseEditTemplate|| UseInlineEditTemplate;

            Label title = null;
            Label desc = null;
            TextBox innerControl = null;
            var setting = this.Field == null ? null : (NumberFieldSetting)this.Field.FieldSetting;
            var digits = Math.Min(setting == null || !setting.Digits.HasValue ? 2 : setting.Digits.Value, 29);
            var format = "F" + digits;

            if (templated)
            {
                title = GetLabelForTitleControl() as Label;
                desc = GetLabelForDescription() as Label;
                innerControl = GetInnerControl() as TextBox;

                if (this.Field != null)
                {
                    if (title != null) 
                        title.Text = this.Field.DisplayName;
                    if (desc != null) 
                        desc.Text = this.Field.Description;
                }

                if (desc != null)
                {
                    var formatDesc = " Valid format is: " + ((decimal)1234.56).ToString(format);
                    var descText = desc.Text;
                    if (string.IsNullOrEmpty(descText) || descText.EndsWith("."))
                        desc.Text = string.Concat(descText, formatDesc).Trim(' ');
                    else
                        desc.Text = string.Concat(descText, ".", formatDesc).Trim(' ');
                }
            }

            if (data == null)
            {
                _inputTextBox.Text = string.Empty;

                if (innerControl != null)
                    innerControl.Text = string.Empty;

                return;
            }

            decimal decimalData;
            var stringData = data as string;
            if (stringData != null)
            {
                if (stringData == string.Empty)
                    decimalData = ActiveSchema.DecimalMinValue;
                else if (!Decimal.TryParse(stringData, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.GetCultureInfo("en-us"), out decimalData))
                    throw new ApplicationException(String.Concat(
                        "Default decimal value is not in a correct format. ContentType: ", this.Field.Content.ContentType.Name,
                        " Field Name: ", this.FieldName));
            }
            else
            {
                decimalData = (decimal)data;
            }

            _inputTextBox.Text = decimalData <= ActiveSchema.DecimalMinValue ? string.Empty : decimalData.ToString(format);

            if (!templated)
                return;
            
            if (innerControl != null) 
                innerControl.Text = decimalData <= ActiveSchema.DecimalMinValue 
                    ? string.Empty 
                    : decimalData.ToString(format);
        }
		
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;

            _inputTextBox.CssClass = String.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-number" : CssClass;
            Controls.Add(_inputTextBox);
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

			if (RenderMode == FieldControlRenderMode.Browse)
				RenderSimple(writer);
			else
				RenderEditor(writer);
		}
        
        // Internals ////////////////////////////////////////////////////////////////////
        private void ManipulateTemplateControls()
        {
            var ic = GetInnerControl() as TextBox;
            if (ic == null)
                return;

            if (Field.ReadOnly)
                ic.Enabled = false;
            else if (ReadOnly)
            {
                ic.Enabled = !ReadOnly;
                ic.EnableViewState = false;
            }

            if (RenderMode != FieldControlRenderMode.InlineEdit)
                return;

            ic.Attributes.Add("Title", String.Concat(Field.DisplayName, " ", Field.Description));
        }
		protected virtual void RenderSimple(HtmlTextWriter writer)
		{
			writer.Write(_inputTextBox.Text);
		}
        protected virtual void RenderEditor(HtmlTextWriter writer)
		{
			if (RenderMode == FieldControlRenderMode.InlineEdit)
            {
                var titleText = String.Concat(this.Field.DisplayName, " ", this.Field.Description);
                _inputTextBox.Attributes.Add("Title", titleText);
            }
            if (Field.ReadOnly)
                writer.Write(_inputTextBox.Text);
            else if (ReadOnly)
            {
                _inputTextBox.Enabled = !this.ReadOnly;
                _inputTextBox.EnableViewState = false;
                _inputTextBox.RenderControl(writer);
            }
            else
                _inputTextBox.RenderControl(writer);
        }

        #region ITemplateFieldControl Members

        public Control GetInnerControl() { return this.FindControlRecursive(InnerControlID); }
        public Control GetLabelForDescription() { return this.FindControlRecursive(DescriptionControlID); }
        public Control GetLabelForTitleControl() { return this.FindControlRecursive(TitleControlID); }

        #endregion
    }
}