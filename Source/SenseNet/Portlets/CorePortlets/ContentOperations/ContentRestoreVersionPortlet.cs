using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;

namespace SenseNet.Portal.Portlets
{
    public class ContentRestoreVersionPortlet : ContextBoundPortlet
    {
        public ContentRestoreVersionPortlet()
        {
            this.Name = "Version restore";
            this.Description = "This portlet allows a specific content version to be restored (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        //================================================================ Properties

        private string _viewPath = "/Root/System/SystemPlugins/Portlets/ContentRestoreVersion/RestoreVersion.ascx";

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

        private Button _restoreButton;
        protected Button RestoreButton
        {
            get
            {
                return _restoreButton ?? (_restoreButton = this.FindControlRecursive("Restore") as Button);
            }
        }

        private Label _contentLabel;
        protected Label ContentLabel
        {
            get { return _contentLabel ?? (_contentLabel = this.FindControlRecursive("ContentName") as Label); }
        }

        private Label _contentVersionLabel;
        protected Label ContentVersionLabel
        {
            get { return _contentVersionLabel ?? (_contentVersionLabel = this.FindControlRecursive("ContentVersion") as Label); }
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
                //ShowError("This version cannot be restored");
                return;
            }

            if (ContentLabel != null)
                ContentLabel.Text = genericContent.DisplayName;

            if (ContentVersionLabel != null)
                ContentVersionLabel.Text = genericContent.Version.ToString();

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers

        protected void RestoreControl_ButtonsAction(object sender, CommandEventArgs e)
        {
            var genericContent = GetContextNode() as GenericContent;
            if (genericContent == null)
                return;

            try
            {
                switch (e.CommandName)
                {
                    case "Restore":
                        genericContent.Save();
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

        //====================================================================== Helper methods

        private void BindEvents()
        {
            if (this.RestoreButton != null)
                this.RestoreButton.Command += RestoreControl_ButtonsAction;
        }

        private void ShowError(string message)
        {
            if (ErrorPlaceholder != null)
                ErrorPlaceholder.Visible = true;

            if (ErrorLabel != null && !string.IsNullOrEmpty(message))
                ErrorLabel.Text = message;
        }
    }
}
