using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:Name ID=\"Name1\" runat=server></{0}:Name>")]
    public class Name : FieldControl, INamingContainer, ITemplateFieldControl
    {
        /* ================================================================================================ Members */
        private string _text;
        private readonly TextBox _shortTextBox;
        private readonly Label _label;
        private readonly TextBox _extensionText;
        private ImageButton _editButton;
        private ImageButton _cancelButton;
        // presence of displayname is determined clientside, since commenting out a displayname fieldcontrol in a contenview leads to false functionality
        private readonly TextBox _displayNameAvailableControl;  
        private string LabelControlID = "LabelControl";
        private string EditButtonControlID = "EditButtonControl";
        private string CancelButtonControlID = "CancelButtonControl";
        private string ExtensionTextControlID = "ExtensionText";
        private string DisplayNameAvailableControlID = "DisplayNameAvailableControl";


        /* ================================================================================================ Public Properties */
        // when set to "true" from a contentview, the fieldcontrol will not set the content's displayname even if the displayname control is not present in the contentview
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool NeverOverrideDisplayName { get; set; }

        // when set to "true" from a contentview, the control is rendered as editable
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool AlwaysEditable { get; set; }


        /* ================================================================================================ Properties */
        protected string FileExtension
        {
            get
            {
                var extension = string.Empty;
                if (_text != null)
                {
                    var index = _text.LastIndexOf('.');
                    if (index != -1 && _text.Length > index + 1)
                        extension = _text.Substring(index + 1);
                }
                return extension;
            }
        }
        protected string FileNameWithoutExtension
        {
            get
            {
                var ext = this.FileExtension;
                if (string.IsNullOrEmpty(ext))
                    return _text;

                return _text.Substring(0, _text.Length - ext.Length - 1);
            }
        }
        protected bool UseExtension
        {
            get
            {
                return this.ContentHandler is SenseNet.ContentRepository.File;
            }
        }

        /* ================================================================================================ Constructor */
        public Name()
        {
            InnerControlID = "InnerShortText";
            _shortTextBox = new TextBox {ID = InnerControlID};
            _label = new Label {ID = LabelControlID};
            _extensionText = new TextBox {ID = ExtensionTextControlID};
            _displayNameAvailableControl = new TextBox {ID = DisplayNameAvailableControlID};
        }


        /* ================================================================================================ Methods */
        public override void SetData(object data)
        {
            if (data == null)
                _text = null;
            else
                _text = data.ToString();

            // get extension
            var extension = this.FileExtension;
            var fileName = this.FileNameWithoutExtension;


            _shortTextBox.Text = UseExtension ? fileName : _text;
            _label.Text = _text;
            _extensionText.Text = UseExtension ? extension : string.Empty;

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            // synchronize data with controls are given in the template
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as TextBox;
            var labelControl = GetLabelControl();
            var extensionTextControl = GetExtensionTextControl() as TextBox;
            if (title != null)
                title.Text = this.Field.DisplayName;
            if (desc != null)
                desc.Text = this.Field.Description;
            if (innerControl != null)
                innerControl.Text = UseExtension ? fileName : _text;
            if (labelControl != null)
                labelControl.Text = _text;
            if (extensionTextControl != null)
                extensionTextControl.Text = UseExtension ? extension : string.Empty;
        }
        public override object GetData()
        {
            var innerControl = _shortTextBox;
            var extensionControl = _extensionText;
            var displayNameAvailableControl = _displayNameAvailableControl;

            if (IsTemplated)
            {
                innerControl = GetInnerControl() as TextBox;
                extensionControl = GetExtensionTextControl();
                displayNameAvailableControl = GetDisplayNameAvailableControl();
            }

            var fileName = string.Empty;
            if (innerControl == null)
                fileName = _shortTextBox.Text;
            else
                fileName = innerControl.Text;

            var extension = string.Empty;
            if (extensionControl == null)
                extension = _extensionText.Text;
            else
                extension = extensionControl.Text;

            var finalName = fileName;

            if (UseExtension && !string.IsNullOrEmpty(extension))
            {
                finalName = string.Concat(fileName, '.', extension);
            }

            // displayname control is available
            var displayNameControlAvailable = false;
            if (displayNameAvailableControl != null)
            {
                if (displayNameAvailableControl.Text != "0")
                    displayNameControlAvailable = true;
            }
            if (!this.NeverOverrideDisplayName)
            {
                if (!displayNameControlAvailable)
                    this.Content["DisplayName"] = finalName;
            }
            return finalName;
        }
        protected override void OnInit(EventArgs e)
        {
            UITools.AddScript("$skin/scripts/sn/SN.ContentName.js");

            base.OnInit(e);

            if (this.ControlMode == FieldControlControlMode.Browse)
                return;

            var innerControl = _shortTextBox;
            var extensionControl = _extensionText;
            var labelControl = _label;
            var editButton = _editButton;
            var cancelButton = _cancelButton;
            var displayNameAvailableControl = _displayNameAvailableControl;

            if (IsTemplated)
            {
                innerControl = GetInnerControl() as TextBox;
                extensionControl = GetExtensionTextControl();
                labelControl = GetLabelControl();
                editButton = GetEditButtonControl();
                cancelButton = GetCancelButtonControl();
                displayNameAvailableControl = GetDisplayNameAvailableControl();
            }

            // extension control should only be visible for files
            var extensionTextId = UseExtension ? extensionControl.ClientID : string.Empty;

            // init javascripts
            if (editButton != null)
                editButton.OnClientClick = string.Format("SN.ContentName.EditUrlName('{0}', '{1}', '{2}', '{3}', '{4}'); return false;", innerControl.ClientID, extensionTextId, labelControl.ClientID, editButton.ClientID, cancelButton.ClientID);

            if (cancelButton != null)
                cancelButton.OnClientClick = string.Format("SN.ContentName.CancelEditingUrlName('{0}', '{1}', '{2}', '{3}', '{4}'); return false;", innerControl.ClientID, extensionTextId, labelControl.ClientID, editButton.ClientID, cancelButton.ClientID);

            // if new content and the displayname field is not visible the urlname field should automatically be editable
            // this scripts also sets state of displayNameAvailableControl, to indicate if displayname is visible in dom
            var isNewContent = (this.Content.Id == 0).ToString().ToLower();
            var editable = AlwaysEditable.ToString().ToLower();


            var initScript =
                string.Format(
                    "SN.ContentName.InitUrlNameControl('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}');",
                    innerControl.ClientID, extensionTextId, labelControl.ClientID, editButton.ClientID, cancelButton.ClientID, displayNameAvailableControl.ClientID, editable, isNewContent);

            UITools.RegisterStartupScript("InitUrlNameControl", initScript, this.Page);

            if (IsTemplated)
                return;

            innerControl.Visible = false;
            innerControl.Width = this.Width;
            innerControl.CssClass = string.IsNullOrEmpty(this.CssClass) ? "sn-ctrl sn-ctrl-text" : this.CssClass;
            Controls.Add(innerControl);
        }
        protected override void OnPreRender(EventArgs e)
        {
            // set label value if text has already been entered
            var innerControl = _shortTextBox;
            var extensionControl = _extensionText;
            var labelControl = _label;

            if (IsTemplated)
            {
                innerControl = GetInnerControl() as TextBox;
                labelControl = GetLabelControl();
                extensionControl = GetExtensionTextControl();
            }

            if (innerControl != null && labelControl != null && extensionControl != null)
            {
                var finalName = innerControl.Text;
                if (!string.IsNullOrEmpty(extensionControl.Text))
                    finalName = string.Concat(innerControl.Text, '.', extensionControl.Text);

                labelControl.Text = finalName;
            }

            ChildControlsCreated = true;
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
                innerShortText.Enabled = false;
                innerShortText.EnableViewState = false;
            }
        }


        /* ================================================================================================ ITemplateFieldControl */
        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }
        public Label GetLabelControl()
        {
            return this.FindControlRecursive(LabelControlID) as Label;
        }
        public TextBox GetExtensionTextControl()
        {
            return this.FindControlRecursive(ExtensionTextControlID) as TextBox;
        }
        public TextBox GetDisplayNameAvailableControl()
        {
            return this.FindControlRecursive(DisplayNameAvailableControlID) as TextBox;
        }
        public ImageButton GetEditButtonControl()
        {
            return this.FindControlRecursive(EditButtonControlID) as ImageButton;
        }
        public ImageButton GetCancelButtonControl()
        {
            return this.FindControlRecursive(CancelButtonControlID) as ImageButton;
        }
        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID) as Label;
        }
        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID) as Label;
        }


        /* ================================================================================================ backward compatibility */
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
    }

}

