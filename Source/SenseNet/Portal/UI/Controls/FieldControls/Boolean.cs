using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.UI.Controls
{
	[ToolboxData("<{0}:Boolean ID=\"Boolean1\" runat=server></{0}:Boolean>")]
	public class Boolean : FieldControl, INamingContainer, ITemplateFieldControl
	{
        // Properties ///////////////////////////////////////////////////////////////////
        private readonly CheckBox _checkBoxCtl;
        // Constructor //////////////////////////////////////////////////////////////////
		public Boolean()
		{
		    InnerControlID = "InnerCheckBox";
            _checkBoxCtl = new CheckBox { ID = InnerControlID };
		}

        // Methods //////////////////////////////////////////////////////////////////////
        public override void SetData(object data)
        {
            _checkBoxCtl.Checked = Convert.ToBoolean(data);

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            // synchronize data with controls are given in the template
            
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as CheckBox;
            if (title != null)
                title.Text = this.Field.DisplayName;
            if (desc != null)
                desc.Text = this.Field.Description;
            if (innerControl != null)
                innerControl.Checked = Convert.ToBoolean(data);

            #endregion
        }

        public override object GetData()
		{
            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return _checkBoxCtl.Checked;
            var innerControl = GetInnerControl() as CheckBox;
            return innerControl != null ? innerControl.Checked : _checkBoxCtl.Checked;

            #endregion
		}

        // Events ///////////////////////////////////////////////////////////////////////
		protected override void OnInit(EventArgs e)
		{
            base.OnInit(e);

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;

		    #region original flow

		    _checkBoxCtl.CssClass = string.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-checkbox" : this.CssClass;
		    Controls.Add(_checkBoxCtl);

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
            var innerCheckBox = GetInnerControl() as CheckBox;
            if (innerCheckBox == null) return;
            if (Field.ReadOnly)
            {
                var p = innerCheckBox.Parent;
                if (p != null)
                {
                    innerCheckBox.Enabled = false;
                    innerCheckBox.EnableViewState = false;
                }
            }
            else if (ReadOnly)
            {
                innerCheckBox.Enabled = !ReadOnly;
                innerCheckBox.EnableViewState = false;
            }
        }
		private void RenderSimple(TextWriter writer)
		{
			writer.Write(_checkBoxCtl.Checked);
		}
		private void RenderEditor(HtmlTextWriter writer)
		{
			if (String.IsNullOrEmpty(this.CssClass))
                _checkBoxCtl.CssClass = "sn-ctrl sn-checkbox";
			if (this.Field.ReadOnly)
			{
				_checkBoxCtl.Enabled = false;
				_checkBoxCtl.EnableViewState = false;
				_checkBoxCtl.RenderControl(writer);
			}
			else if (this.ReadOnly)
			{
				_checkBoxCtl.Enabled = !this.ReadOnly;
				_checkBoxCtl.EnableViewState = false;
				_checkBoxCtl.RenderControl(writer);
			}
			else
			{
				// render read/write control
				_checkBoxCtl.RenderControl(writer);
			}
		}


        #region ITemplateFieldControl Members

        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }

        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID);
        }

        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID);
        }

        #endregion
    }
}