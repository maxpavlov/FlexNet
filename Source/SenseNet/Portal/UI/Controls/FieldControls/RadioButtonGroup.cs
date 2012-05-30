using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using SenseNet.ContentRepository.Fields;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:RadioButtonGroup ID=\"RadioButtonGroup1\" runat=server></{0}:RadioButtonGroup>")]
	public class RadioButtonGroup : ChoiceControl, INamingContainer, ITemplateFieldControl
	{
        // Members //////////////////////////////////////////////////////////////////////
        private readonly string ExtraTextBoxID = "ExtraTextBox";
		private readonly RadioButtonList _listControl;
		private readonly TextBox _extraTextBox;
		protected override ListItemCollection InnerListItemCollection { get { return _listControl.Items; } }
        public ListItemCollection ListItems { get { return _listControl.Items; } }
        [PersistenceMode(PersistenceMode.Attribute)] public int RepeatColumns { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)] public RepeatDirection RepeatDirection { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)] public RepeatLayout RepeatLayout { get; set; }
        // Constructor //////////////////////////////////////////////////////////////////
		public RadioButtonGroup()
		{
            _listControl = new RadioButtonList { ID = InnerControlID };
            _extraTextBox = new TextBox { ID = ExtraTextBoxID };
			_extraTextBox.Visible = false;
		}
        // Methods //////////////////////////////////////////////////////////////////////
        public override void SetData(object data)
        {
            base.SetData(data);

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;
            // synchronize data with controls are given in the template
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as RadioButtonList;
            if (title != null) title.Text = this.Field.DisplayName;
            if (desc != null) desc.Text = this.Field.Description;
            if (innerControl == null) return;
            innerControl.Items.Clear();
            if (data != null)
                BuildControl(innerControl.Items, (List<string>)data);

            #endregion
        }
        public override object GetData()
        {
            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return base.GetData();
            var innerControl = GetInnerControl() as RadioButtonList;
            return innerControl != null ? GetSelectedOptions(innerControl.Items) : base.GetData();

            #endregion
        }
        // Events ///////////////////////////////////////////////////////////////////////
		protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;
            #region original
            _listControl.CssClass = String.IsNullOrEmpty(this.CssClass) ? "sn-radiogroup" : this.CssClass;
            _listControl.RepeatDirection = this.RepeatDirection;
            _listControl.RepeatColumns = this.RepeatColumns;
            _listControl.RepeatLayout = this.RepeatLayout;
            this.Controls.Add(_listControl);
            this.Controls.Add(_extraTextBox);
            _listControl.ID = String.Concat(this.ID, "_", this.ContentHandler.Id.ToString());
            _extraTextBox.ID = String.Concat(_listControl.ID, "_extraValue");
            #endregion
        }
        protected override void RenderContents(System.Web.UI.HtmlTextWriter writer)
        {
            #region template

            if (UseBrowseTemplate)
            {
                FillBrowseControls();
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

            #region original

            if (this.RenderMode == FieldControlRenderMode.Browse)
                RenderSimple(writer);
            else
                RenderEditor(writer);

            #endregion
		}

        #region template

        private void ManipulateTemplateControls()
        {

            var et = GetExtraTextBox() as TextBox;
            var ic = GetInnerControl() as RadioButtonList;

            if (ic == null) return;

            if (Field.ReadOnly)
            {
                ic.Enabled = false;
                ic.EnableViewState = true;
                if (AllowExtraValue && et != null)
                {
                    et.Enabled = ic.Enabled;
                    et.EnableViewState = ic.EnableViewState;
                }
            }
            else if (ReadOnly)
            {
                ic.Enabled = false;
                ic.EnableViewState = true;
                if (AllowExtraValue && et != null)
                {
                    et.Enabled = ic.Enabled;
                    et.EnableViewState = ic.EnableViewState;
                }
            }

            if (et != null) et.Visible = AllowExtraValue;

            if (UseInlineEditTemplate)
                ic.Attributes.Add("Title", String.Concat(Field.DisplayName, " ", Field.Description));

        }

        #endregion

		private void RenderSimple(HtmlTextWriter writer)
		{
            writer.Write(SelectedValueType.Equals(SelectedValueTypes.Value) ?
                String.Join(", ", GetSelectedItems(_listControl.Items, true).ToArray()) :
                String.Join(", ", GetSelectedItems(_listControl.Items, false).ToArray()));
		}
		private void RenderEditor(HtmlTextWriter writer)
		{
            _extraTextBox.Visible = this.AllowExtraValue;

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
				if (this.AllowExtraValue == true)
				{
					_extraTextBox.Enabled = _listControl.Enabled;
					_extraTextBox.EnableViewState = _listControl.EnableViewState;
					_extraTextBox.RenderControl(writer);
				}
            }
            else if (this.ReadOnly)
            {
                _listControl.Enabled = !this.ReadOnly;
                _listControl.EnableViewState = false;
                _listControl.RenderControl(writer);
				if (this.AllowExtraValue == true)
				{
					_extraTextBox.Enabled = _listControl.Enabled;
					_extraTextBox.EnableViewState = _listControl.EnableViewState;
					_extraTextBox.RenderControl(writer);
				}
			}
            else
            {
                // render read/write control
                _listControl.RenderControl(writer);
				if (this.AllowExtraValue == true)
				{
					_extraTextBox.RenderControl(writer);
				}
			}
        }

        protected override string GetExtraValue()
        {
            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                var et = GetExtraTextBox() as TextBox;
                return et == null ? _extraTextBox.Text : et.Text;
            }
            return _extraTextBox.Text;
        }
        protected override void SetExtraValue(string value)
        {
            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                var et = GetExtraTextBox() as TextBox;
                if (et != null) et.Text = value;
                return;
            }
            _extraTextBox.Text = value;
        }

        #region ITemplateFieldControl Members
        public Control GetInnerControl() { return this.FindControlRecursive(InnerControlID); }
        public Control GetLabelForDescription() { return this.FindControlRecursive(DescriptionControlID); }
        public Control GetLabelForTitleControl() { return this.FindControlRecursive(TitleControlID); }
        #endregion
        public Control GetExtraTextBox() { return this.FindControlRecursive(ExtraTextBoxID); }
    }
}