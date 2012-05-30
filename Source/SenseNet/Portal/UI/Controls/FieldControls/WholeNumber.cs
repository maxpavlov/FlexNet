using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.UI.Controls
{
	[ToolboxData("<{0}:WholeNumber ID=\"WholeNumber1\" runat=server></{0}:WholeNumber>")]
	public class WholeNumber : FieldControl, INamingContainer, ITemplateFieldControl
	{
        protected string PercentageControlID = "LabelForPercentage";

        // Fields ///////////////////////////////////////////////////////////////////////
		private TextBox _inputTextBox;

        // Constructor //////////////////////////////////////////////////////////////////
		public WholeNumber()
		{
            InnerControlID = "InnerWholeNumber";
			_inputTextBox = new TextBox { ID = InnerControlID };
		}

        // Methods //////////////////////////////////////////////////////////////////////
        public override void SetData(object data)
        {
            if (data == null)
            {
                _inputTextBox.Text = string.Empty;
            }
            else
            {
                _inputTextBox.Text = Convert.ToInt32(data) == int.MinValue ? string.Empty : data.ToString();
            }

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as TextBox;
            var perc = GetLabelForPercentageControl();
            
            if (title != null)
                title.Text = this.Field.DisplayName;
            
            if (desc != null)
                desc.Text = this.Field.Description;
            
            if (innerControl != null)
                innerControl.Text = Convert.ToString(_inputTextBox.Text);

            if (perc != null)
            {
                perc.Text = GetPercentageSign();
                perc.Visible = !string.IsNullOrEmpty(perc.Text);
            }

            #endregion
        }
        public override object GetData()
		{
            var innerControl = GetInnerControl() as TextBox;

            if ((!UseBrowseTemplate && !UseEditTemplate && !UseInlineEditTemplate) || innerControl == null)
            {
                #region original

                if (_inputTextBox.Text.Length == 0)
                    return null;

                return Convert.ToInt32(_inputTextBox.Text);

                #endregion
            }

            if (innerControl.Text.Length == 0) 
                return null;
            
            return Convert.ToInt32(innerControl.Text);
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

		    _inputTextBox.CssClass = String.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-number" : this.CssClass;
		    Controls.Add(_inputTextBox);
		    //_inputTextBox.ID = String.Concat(this.ID, "_", this.ContentHandler.Id.ToString());

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
		private void RenderSimple(HtmlTextWriter writer)
		{
			writer.Write(_inputTextBox.Text);

            RenderPercentage(writer);
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

		    RenderPercentage(writer);
		}

        private void ManipulateTemplateControls()
        {
            //
            //  This method is needed to ensure the common fieldcontrol logic.
            //
            var innerWholeNumber = GetInnerControl() as TextBox;
            var lt = GetLabelForTitleControl() as Label;
            var ld = GetLabelForDescription() as Label;

            if (innerWholeNumber == null) return;

            if (Field.ReadOnly)
            {
                var p = innerWholeNumber.Parent;
                if (p != null)
                {
                    p.Controls.Remove(innerWholeNumber);
                    if (lt != null) lt.AssociatedControlID = string.Empty;
                    if (ld != null) ld.AssociatedControlID = string.Empty;
                    p.Controls.Add(new LiteralControl(innerWholeNumber.Text));
                }
            }
            else if (ReadOnly)
            {
                innerWholeNumber.Enabled = !ReadOnly;
                innerWholeNumber.EnableViewState = false;
            }

            if (RenderMode != FieldControlRenderMode.InlineEdit)
                return;

            innerWholeNumber.Attributes.Add("Title", String.Concat(Field.DisplayName, " ", Field.Description));
   
        }

        private void RenderPercentage(HtmlTextWriter writer)
        {
            writer.Write(GetPercentageSign());
        }

        private string GetPercentageSign()
        {
            var fs = this.Field.FieldSetting as IntegerFieldSetting;

            if (fs == null)
                return string.Empty;

            if (fs.ShowAsPercentage.HasValue && fs.ShowAsPercentage.Value)
                return "%";

            return string.Empty;
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

        public override object Data
        {
            get
            {
                return _inputTextBox.Text;
            }
        }

        public Label GetLabelForPercentageControl()
        {
            return this.FindControlRecursive(PercentageControlID) as Label;
        }
    }
}