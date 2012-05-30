using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using System.ComponentModel;

namespace SenseNet.Portal.Portlets
{
    public class ContentApprovalPortlet : ContextBoundPortlet
    {
        

        public ContentApprovalPortlet()
        {
            this.Name = "Approval";
            this.Description = "This portlet allows a content to be approved or rejected (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        //================================================================ Properties

        private string _viewPath = "/Root/System/SystemPlugins/Portlets/ContentApproval/Approval.ascx";
        private bool _needValidation = false;

        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the UI elements of the approval dialog")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPath
        {
            get { return _viewPath; }
            set { _viewPath = value; }
        }

        [WebDisplayName("Publish with validation")]
        [WebDescription("Set this checkbox for content validation.")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(200)]
        public bool NeedValidation
        {
            get { return _needValidation; }
            set { _needValidation = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        private Button _approveButton;
        protected Button ApproveButton
        {
            get
            {
                return _approveButton ?? (_approveButton = this.FindControlRecursive("Approve") as Button);
            }
        }

        private Button _rejectButton;
        protected Button RejectButton
        {
            get
            {
                return _rejectButton ?? (_rejectButton = this.FindControlRecursive("Reject") as Button);
            }
        }

        private Label _contentLabel;
        protected Label ContentLabel
        {
            get { return _contentLabel ?? (_contentLabel = this.FindControlRecursive("ContentName") as Label); }
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
                var viewControl = Page.LoadControl(ViewPath) as UserControl;
                if (viewControl != null)
                {
                    Controls.Add(viewControl);
                    BindEvents();
                }
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            var genericContent = GetContextNode() as GenericContent;
            if (genericContent == null)
            {
                ShowError("This type of content cannot be approved");
                return;
            }

            if (ContentLabel != null)
                ContentLabel.Text = genericContent.DisplayName;

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers

        protected void ApprovalControl_ButtonsAction(object sender, CommandEventArgs e)
        {
            var genericContent = GetContextNode() as GenericContent;
            if (genericContent == null)
                return;

            try
            {
                if (NeedValidation)
                {
                    var cnt = SenseNet.ContentRepository.Content.Create(genericContent);
                    if (!cnt.IsValid)
                    {
                        ShowError("Current content is not valid. Please edit content and fix the errors.");
                    }
                }
                switch (e.CommandName)
                {
                    case "Approve":
                        genericContent.Approve();
                        break;
                    case "Reject":
                        genericContent.Reject();
                        break;
                    default:
                        throw new InvalidOperationException("Unknown command");
                }

                CallDone();
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);

                ShowError(ex.Message);
            }
        }

        private void BindEvents()
        {
            if (this.ApproveButton != null)
                this.ApproveButton.Command += ApprovalControl_ButtonsAction;

            if (this.RejectButton != null)
                this.RejectButton.Command += ApprovalControl_ButtonsAction;
        }

        //====================================================================== Helper methods

        private void ShowError(string message)
        {
            if (ErrorPlaceholder != null)
                ErrorPlaceholder.Visible = true;

            if (ErrorLabel != null && !string.IsNullOrEmpty(message))
                ErrorLabel.Text = message;
        }
    }
}
