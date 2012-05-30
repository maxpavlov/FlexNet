using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository.Storage;
using System.Text;
using SenseNet.Portal.UI;
using System.ComponentModel;

namespace SenseNet.Portal.Portlets
{
    public class ContentCopyPortlet : ContentCollectionPortlet
    {
        public ContentCopyPortlet()
        {
            this.Name = "Copy";
            this.Description = "This portlet handles content copy operations (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);

            Cacheable = false;   // by default, caching is switched off
            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Cache };
        }

        private string _viewPathCopyTo = "/Root/System/SystemPlugins/Portlets/ContentCopy/CopyTo.ascx";
        private string _viewPathCopy = "/Root/System/SystemPlugins/Portlets/ContentCopy/Copy.ascx";

        //================================================================ Portlet properties

        [WebDisplayName("View path for 'copy under current content'")]
        [WebDescription("Path of the .ascx user control which provides the UI elements for the 'COPY TO' behavior of the copy dialog")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPathCopyTo
        {
            get { return _viewPathCopyTo; }
            set { _viewPathCopyTo = value; }
        }

        [WebDisplayName("View path for 'copy current content'")]
        [WebDescription("Path of the .ascx user control which provides the UI elements for the 'COPY' behavior of the copy dialog")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPathCopy
        {
            get { return _viewPathCopy; }
            set { _viewPathCopy = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        //================================================================ Controls

        private Label _contentLabel;
        protected Label ContentLabel
        {
            get { return _contentLabel ?? (_contentLabel = this.FindControlRecursive("ContentName") as Label); }
        }

        private Button _copyToButton;
        protected Button CopyToButton
        {
            get
            {
                return _copyToButton ?? (_copyToButton = this.FindControlRecursive("CopyToButton") as Button);
            }
        }

        private Button _copyCurrentButton;
        protected Button CopyCurrentButton
        {
            get
            {
                return _copyCurrentButton ?? (_copyCurrentButton = this.FindControlRecursive("CopyCurrentButton") as Button);
            }
        }

        private Button _doneButton;
        protected Button DoneButton
        {
            get
            {
                return _doneButton ?? (_doneButton = this.FindControlRecursive("DoneButton") as Button);
            }
        }

        private Button _cancelButton;
        protected Button CancelButton
        {
            get
            {
                return _cancelButton ?? (_cancelButton = this.FindControlRecursive("CancelButton") as Button);
            }
        }

        private TextBox _targetBox;
        protected TextBox CopyTargetTextBox
        {
            get
            {
                return _targetBox ?? (_targetBox = this.FindControlRecursive("CopyTargetTextBox") as TextBox);
            }
        }

        private Button _openPickerButton;
        protected Button OpenPickerButton
        {
            get
            {
                return _openPickerButton ?? (_openPickerButton = this.FindControlRecursive("OpenPickerButton") as Button);
            }
        }

        private Panel _panelCopy;
        protected Panel CopyPanel
        {
            get
            {
                return _panelCopy ?? (_panelCopy = this.FindControlRecursive("CopyPanel") as Panel);
            }
        }

        private Panel _panelMsg;
        protected Panel MessagePanel
        {
            get
            {
                return _panelMsg ?? (_panelMsg = this.FindControlRecursive("MessagePanel") as Panel);
            }
        }

        private PlaceHolder _plcError;
        protected PlaceHolder ErrorPlaceholder
        {
            get
            {
                return _plcError ?? (_plcError = this.FindControlRecursive("ErrorPanel") as PlaceHolder);
            }
        }

        private Label _errorLabel;
        protected Label ErrorLabel
        {
            get
            {
                return _errorLabel ?? (_errorLabel = this.FindControlRecursive("ErrorLabel") as Label);
            }
        }

        //================================================================ Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();

            try
            {
                var viewControl = RequestIdList.Count == 0 ?
                    Page.LoadControl(ViewPathCopy) as UserControl :
                    Page.LoadControl(ViewPathCopyTo) as UserControl;
                
                if (viewControl == null)
                    return;
                
                Controls.Add(viewControl);

                var genericContent = GetContextNode() as GenericContent;
                if (genericContent == null)
                    return;
                
                if (ContentLabel != null)
                    ContentLabel.Text = genericContent.DisplayName;

                BindEvents();
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers

        protected void CopyToButton_Click(object sender, EventArgs e)
        {
            try
            {
                HideErrorPanel();

                var contextNode = ContextNode;
                if (contextNode == null)
                    return;

                var exceptions = new List<Exception>();

                Node.Copy(RequestIdList, contextNode.Path, ref exceptions);

                if (exceptions.Count > 0)
                {
                    var sbError = new StringBuilder();
                    foreach (var exception in exceptions)
                    {
                        sbError.AppendFormat(" {0}", exception.Message);
                    }

                    SetError(sbError.ToString());
                }
                else 
                {
                    SetControlVisibility(true);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        protected void CopyCurrentButton_Click(object sender, EventArgs e)
        {
            try
            {
                HideErrorPanel();

                if (CopyTargetTextBox == null)
                    return;

                if (string.IsNullOrEmpty(CopyTargetTextBox.Text))
                {
                    SetError("Please choose a folder or content list to copy to.");
                    return;
                }

                var target = Node.LoadNode(CopyTargetTextBox.Text);
                if (target == null)
                {
                    SetError("Target is invalid.");
                    return;
                }

                var contextNode = ContextNode;
                if (contextNode == null)
                {
                    SetError("Context node is invalid.");
                    return;
                }

                Node.Copy(contextNode.Path, target.Path);

                SetControlVisibility(true);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                SetError(ex.Message);
            }
        }

        //====================================================================== Helper methods

        protected void BindEvents()
        {
            if (CopyToButton != null)
            {
                CopyToButton.Click += CopyToButton_Click;
                CopyToButton.Visible = RequestIdList.Count > 0;
            }

            if (CopyCurrentButton != null)
            {
                CopyCurrentButton.Click += CopyCurrentButton_Click;
                CopyCurrentButton.Visible = RequestIdList.Count == 0;
            }

            if (OpenPickerButton != null)
            {
                UITools.AddPickerCss();
                UITools.AddScript(UITools.ClientScriptConfigurations.SNPickerPath);
                OpenPickerButton.OnClientClick = GetOpenContentPicklerScript();
            }
        }

        protected void SetControlVisibility(bool finished)
        {
            if (MessagePanel != null)
                MessagePanel.Visible = finished;
            if (CopyPanel != null)
                CopyPanel.Visible = !finished;

            if (CancelButton != null)
                CancelButton.Visible = !finished;
            if (CopyToButton != null)
                CopyToButton.Visible = !finished;
            if (CopyCurrentButton != null)
                CopyCurrentButton.Visible = !finished;

            if (DoneButton != null)
                DoneButton.Visible = finished;
        }

        private void HideErrorPanel()
        {
            if (ErrorPlaceholder != null)
                ErrorPlaceholder.Visible = false;
            if (ErrorLabel != null)
                ErrorLabel.Visible = false;
        }

        private string GetOpenContentPicklerScript()
        {
            if (CopyTargetTextBox == null)
                return string.Empty;

            var callBack = string.Format("function(resultData) {{ if (!resultData) return; $('#{0}').val(resultData[0].Path); }}",
                CopyTargetTextBox.ClientID);

            var script = string.Format(@"javascript: SN.PickerApplication.open({{ MultiSelectMode: 'none', callBack: {0} }}); return false;",
                callBack);

            return script;
        }

        private void SetError(string errorMessage)
        {
            if (ErrorLabel == null) 
                return;

            ErrorLabel.Visible = true;

            if (ErrorPlaceholder != null)
                ErrorPlaceholder.Visible = true;

            ErrorLabel.Text = errorMessage;
        }
    }
}
