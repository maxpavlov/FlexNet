using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.UI.PortletFramework;
using System.ComponentModel;
using System.Web.UI.WebControls;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository;
using System.Web.UI;
using SenseNet.Portal.UI.Controls;
using SenseNet.Workflow;
using SenseNet.ContentRepository.Storage.Schema;


namespace SenseNet.Portal.Portlets
{
    public class AbortWorkflowPortlet : ContextBoundPortlet
    {
        public AbortWorkflowPortlet()
        {
            this.Name = "Abort workflow";
            this.Description = "This portlet allows a workflow to be aborted (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Workflow);
        }

        //================================================================ Properties

        private string _viewPath = "/Root/System/SystemPlugins/Portlets/WorkflowAbort/WorkflowAbort.ascx";

        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(100)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ViewPath
        {
            get { return _viewPath; }
            set { _viewPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        private Button _abortButton;
        protected Button AbortButton
        {
            get
            {
                return _abortButton ?? (_abortButton = this.FindControlRecursive("Abort") as Button);
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

            var workflow = GetContextNode() as WorkflowHandlerBase;
            if (workflow == null)
            {
                ShowError("This type of content cannot be aborted");
                return;
            }

            if (ContentLabel != null)
                ContentLabel.Text = workflow.DisplayName;

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers

        protected void Abort_ButtonsAction(object sender, CommandEventArgs e)
        {
            var workflow = GetContextNode() as WorkflowHandlerBase;
            if (workflow == null)
                return;

            try
            {
                if (!workflow.Security.HasPermission(PermissionType.Save))
                {
                    ShowError("You don't have enough permission to abort this workflow!");
                    return;
                }

                InstanceManager.Abort(workflow, WorkflowApplicationAbortReason.ManuallyAborted);
               
                CallDone(false);
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);

                ShowError(ex.Message);
            }
        }

        private void BindEvents()
        {
            if (this.AbortButton != null)
                this.AbortButton.Command += Abort_ButtonsAction;
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