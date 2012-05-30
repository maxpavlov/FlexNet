using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using SenseNet.ContentRepository.Fields;
using System.Globalization;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:Password ID=\"Password1\" runat=server></{0}:Password>")]
    public class Password : FieldControl, INamingContainer, ITemplateFieldControl
    {
        // Fields ///////////////////////////////////////////////////////////////////////
		public const string DefaultStars = "*******";
        private readonly TextBox _pwdTextBox1;
        protected string InnerControl2ID = "InnerControl";
        private readonly TextBox _pwdTextBox2;
        private PasswordField.PasswordData _originalData;

        [PersistenceMode(PersistenceMode.Attribute)]
        public int MaxLength { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public int PasswordMinLength { get; set; }

        // Constructor //////////////////////////////////////////////////////////////////
        public Password()
		{
            InnerControlID = "InnerPassword1";
            InnerControl2ID = "InnerPassword2";
            _pwdTextBox1 = new TextBox { ID = InnerControlID };
            _pwdTextBox2 = new TextBox { ID = InnerControl2ID };
		}

        // Methods //////////////////////////////////////////////////////////////////////
		public override void SetData(object data)
		{
		    _originalData = data as PasswordField.PasswordData;
		    //SetStarsOnTextBoxes();

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var pwd1 = GetInnerControl() as TextBox;

		    var title2 = GetLabel(String.Concat(TitleControlID, "2")) as Label;
            var desc2 = GetLabel(String.Concat(DescriptionControlID, "2")) as Label;
            var pwd2 = GetPwdControl(InnerControl2ID) as TextBox;

            if (title != null) title.Text = this.Field.DisplayName;
            if (desc != null) desc.Text = this.Field.Description;
            if (pwd1 != null) SetStarsOnTextBox(pwd1);

            var passwordFieldSetting = this.Field.FieldSetting as PasswordFieldSetting;
            if (passwordFieldSetting != null && passwordFieldSetting.ReenterDescription != null && desc2 != null)
                desc2.Text = passwordFieldSetting.ReenterDescription;

            if (passwordFieldSetting != null && passwordFieldSetting.ReenterTitle != null && title2 != null)
                title2.Text = passwordFieldSetting.ReenterTitle;

            if (pwd2 != null) SetStarsOnTextBox(pwd2);

            #endregion
		}
        public override object GetData()
        {
            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return CheckPasswords(_pwdTextBox1.Text.Trim(), _pwdTextBox2.Text.Trim());

            var passwordControl1 = GetInnerControl() as TextBox;
            var passwordControl2 = GetPwdControl(InnerControl2ID) as TextBox;
            if (passwordControl1 != null && passwordControl2 != null)
                return CheckPasswords(passwordControl1.Text.Trim(), passwordControl2.Text.Trim());    
            
            return _originalData;
        }

        // Events ///////////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;

            #region original flow

            var cssClass = string.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-text" : CssClass;
            
            // set webcontrol datas
            _pwdTextBox1.Width = this.Width;
            _pwdTextBox1.MaxLength = this.MaxLength;
            _pwdTextBox1.CssClass = cssClass;
            _pwdTextBox1.TextMode = TextBoxMode.Password;

            _pwdTextBox2.Width = this.Width;
            _pwdTextBox2.MaxLength = this.MaxLength;
            _pwdTextBox2.CssClass = cssClass;
            _pwdTextBox2.TextMode = TextBoxMode.Password;
            
            this.Controls.Add(_pwdTextBox1);
            this.Controls.Add(_pwdTextBox2);

            SetStarsOnTextBox(_pwdTextBox1);
            SetStarsOnTextBox(_pwdTextBox2);

            #endregion
            
            //_pwdTextBox1.ID = String.Concat(this.ID, "_", this.ContentHandler.Id.ToString());
            //_pwdTextBox2.ID = String.Concat(this.ID, "_", this.ContentHandler.Id.ToString(),"_2");
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
			{
				writer.Write(DefaultStars);
				return;
			}
            // first part of the Password layout (title+description+passwordcontrol)
            if (RenderMode != FieldControlRenderMode.Browse)
                this.RenderBeginTag(writer);
            this.RenderContentsInternal(writer, _pwdTextBox1, false);
            if (this.HasError)
                RenderErrorMessage(writer);
            if (RenderMode != FieldControlRenderMode.Browse)
                this.RenderEndTag(writer);

            // second part of the Password layout (title+description+REENTERpasswordcontrol)
            if (RenderMode != FieldControlRenderMode.Browse)
                this.RenderBeginTagInternal(writer);
            this.RenderContentsInternal(writer, _pwdTextBox2, true);
            if (this.HasError)
                RenderErrorMessage(writer);
            if (RenderMode != FieldControlRenderMode.Browse)
                this.RenderEndTag(writer);
        }

        
        #region backward compatibility

        protected virtual void RenderBeginTagInternal(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), this.InputUnitCssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-label");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            RenderFieldTitleReenter(writer);
            writer.WriteBreak();
            RenderFieldDescriptionReenter(writer);

            writer.RenderEndTag(); // sn-iu-label end

            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-control");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }
        protected virtual void RenderFieldDescriptionReenter(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-desc");
            writer.AddAttribute(HtmlTextWriterAttribute.For.ToString(), String.Concat("editor_", this.ClientID));
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            var passwordFieldSetting = this.Field.FieldSetting as PasswordFieldSetting;
            if (passwordFieldSetting != null && passwordFieldSetting.ReenterDescription != null)
                writer.Write(passwordFieldSetting.ReenterDescription);
            else if (this.Field.Description != null)
                writer.Write(this.Field.Description);
            writer.RenderEndTag();
        }
        protected virtual void RenderFieldTitleReenter(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class.ToString(), "sn-iu-title");
            writer.AddAttribute(HtmlTextWriterAttribute.For.ToString(), String.Concat("editor_", this.ClientID));
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            PasswordFieldSetting passwordFieldSetting = this.Field.FieldSetting as PasswordFieldSetting;
            if (passwordFieldSetting != null && passwordFieldSetting.ReenterTitle != null)
                writer.Write(passwordFieldSetting.ReenterTitle);
            else if (this.Field.DisplayName != null)
                writer.Write(this.Field.DisplayName);
            writer.RenderEndTag();
        }
        protected virtual void RenderContentsInternal(HtmlTextWriter writer, TextBox ctl, bool reenterPassword)
        {
            if (this.RenderMode == FieldControlRenderMode.InlineEdit)
            {
                var titleText = String.Concat(this.Field.DisplayName, " ", this.Field.Description);
                ctl.Attributes.Add("Title", titleText);
            }

            if (this.Field.ReadOnly)
                writer.Write(ctl.Text);
            else if (this.ReadOnly)
            {
                ctl.Enabled = !this.ReadOnly;
                ctl.EnableViewState = false;
                ctl.RenderControl(writer);
            }
            else
                ctl.RenderControl(writer);
        }



        #endregion

        // Internals ////////////////////////////////////////////////////////////////////
        private void ManipulateTemplateControls()
        {
            var title = string.Empty;
            var description = string.Empty;

            // read controls
            var title1 = GetLabelForTitleControl() as Label;
            var desc1 = GetLabelForDescription() as Label;
            var pwd1 = GetInnerControl() as TextBox;
            var title2 = GetLabel(String.Concat(TitleControlID, "2")) as Label;
            var desc2 = GetLabel(String.Concat(DescriptionControlID, "2")) as Label;
            var pwd2 = GetPwdControl(InnerControl2ID) as TextBox;

            // set control values
            if (title1 != null) title1.Text = this.Field.DisplayName;
            if (desc1 != null) desc1.Text = this.Field.Description;
            if (pwd1 != null) SetStarsOnTextBox(pwd1);

            var passwordFieldSetting = this.Field.FieldSetting as PasswordFieldSetting;
            if (passwordFieldSetting != null && passwordFieldSetting.ReenterDescription != null && desc2 != null)
                desc2.Text = passwordFieldSetting.ReenterDescription;
            if (passwordFieldSetting != null && passwordFieldSetting.ReenterTitle != null && title2 != null)
                title2.Text = passwordFieldSetting.ReenterTitle;
            if (pwd2 != null) SetStarsOnTextBox(pwd2);

            if (RenderMode == FieldControlRenderMode.Browse)
                return;
            if (passwordFieldSetting != null)
            {
                title = passwordFieldSetting.ReenterTitle;
                description = passwordFieldSetting.ReenterDescription;
            }
            if (pwd1 != null) SetTitleAttribute(pwd1, title, description);
            if (pwd2 != null) SetTitleAttribute(pwd2, title, description);


        }
        private static void SetTitleAttribute(WebControl control, string title, string description)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            if (title == null)
                throw new ArgumentNullException("title");
            if (description == null)
                throw new ArgumentNullException("description");

            control.Attributes.Add("Title", String.Concat(title, " ", description));
        }
        private static void SetStarsOnTextBox(WebControl textBox)
        {
            if (textBox == null)
                throw new ArgumentNullException("textBox");

            textBox.Attributes.Add("value", DefaultStars);
            textBox.Attributes.Add("onfocus", String.Concat("if(this.value=='", DefaultStars, "'){this.value='';}"));
        }
        private object CheckPasswords(string pwd, string pwd2)
        {
            // each control hasn't got the same value
            if (!pwd.Equals(pwd2))
                throw new FieldControlDataException(this, "PasswordsDoNotMatch", "Password inputs are not equal");

            // user has not been changed the password.
            if (pwd.Equals(DefaultStars) || pwd.Length == 0)
                return _originalData;

            // user has been changed the password.
            return new PasswordField.PasswordData { Text = pwd.Trim() };
        }
        private Control GetPwdControl(string id)
        {
            return this.FindControlRecursive(id) as TextBox;
        }
        private Control GetLabel(string id)
        {
            return this.FindControlRecursive(id) as Label;
        }

        #region ITemplateFieldControl Members

        public Control GetInnerControl()
        {
            return GetPwdControl(InnerControlID);
        }

        public Control GetLabelForDescription()
        {
            return GetLabel(DescriptionControlID);
        }

        public Control GetLabelForTitleControl()
        {
            return GetLabel(TitleControlID);
        }

        #endregion

    }
}