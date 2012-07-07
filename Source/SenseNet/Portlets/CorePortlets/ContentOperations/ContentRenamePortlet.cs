using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using System.ComponentModel;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.Portlets
{
    public class ContentRenamePortlet : ContextBoundPortlet
    {
        private string _viewPath = "/Root/System/SystemPlugins/Portlets/ContentRename/Rename.ascx";
        private string _contentViewPath = "$skin/contentviews/Rename.ascx";

        public ContentRenamePortlet()
        {
            this.Name = "Rename";
            this.Description = "This portlet allows the user to rename a content (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        //================================================================ Portlet properties

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

        [WebDisplayName("Rename view path")]
        [WebDescription("Path of the .ascx content view which displays the name and display name fields")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [WebOrder(110)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        public string ContentViewPath
        {
            get { return _contentViewPath; }
            set { _contentViewPath = value; }
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

        private Button _renameButton;
        protected Button RenameButton
        {
            get
            {
                return _renameButton ?? (_renameButton = this.FindControlRecursive("RenameButton") as Button);
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

        private PlaceHolder _contentViewPlaceHolder;
        protected PlaceHolder ContentViewPlaceHolder
        {
            get
            {
                return _contentViewPlaceHolder ?? (_contentViewPlaceHolder = this.FindControlRecursive("ContentViewPlaceHolder") as PlaceHolder);
            }
        }

        protected ContentView RenameContentView { get; set; }

        private PlaceHolder _displayNamePlaceHolder;
        protected PlaceHolder DisplayNamePlaceHolder
        {
            get
            {
                return _displayNamePlaceHolder ?? (_displayNamePlaceHolder = RenameContentView.FindControlRecursive("DisplayNamePlaceHolder") as PlaceHolder);
            }
        }

        private PlaceHolder _namePlaceHolder;
        protected PlaceHolder NamePlaceHolder
        {
            get
            {
                return _namePlaceHolder ?? (_namePlaceHolder = RenameContentView.FindControlRecursive("NamePlaceHolder") as PlaceHolder);
            }
        }

        private Name _nameControl;
        protected Name NameControl
        {
            get
            {
                return _nameControl ?? (_nameControl = RenameContentView.FindControlRecursive("UrlName") as Name);
            }
        }

        private DisplayName _displayNameControl;
        protected DisplayName DisplayNameControl
        {
            get
            {
                return _displayNameControl ?? (_displayNameControl = RenameContentView.FindControlRecursive("Name") as DisplayName);
            }
        }


        //================================================================ Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();

            try
            {
                var viewControl = Page.LoadControl(ViewPath) as UserControl;
                if (viewControl == null)
                    return;

                Controls.Add(viewControl);

                // load rename contentview
                if (this.ContentViewPlaceHolder != null)
                {
                    var node = GetContextNode();
                    var content = SenseNet.ContentRepository.Content.Create(node);
                    this.RenameContentView = ContentView.Create(content, this.Page, ViewMode.InlineEdit, this.ContentViewPath);

                    // manipulate contentview controls according to CTD visibility settings
                    if (content.Fields["DisplayName"].FieldSetting.VisibleEdit == SenseNet.ContentRepository.Schema.FieldVisibility.Hide)
                    {
                        DisplayNamePlaceHolder.Parent.Controls.Remove(DisplayNamePlaceHolder);
                        NameControl.AlwaysEditable = true;
                    }
                    this.ContentViewPlaceHolder.Controls.Add(this.RenameContentView);
                }
                
                BindEvents();
            }
            catch (Exception exc)
            {
                Logger.WriteException(exc);
            }

            ChildControlsCreated = true;
        }

        //====================================================================== Event handlers

        protected void RenameButton_Click(object sender, EventArgs e)
        {
            try
            {
                HideErrorPanel();

                var contextNode = ContextNode;
                if (contextNode == null)
                    return;

                var pageBase = this.Page as PageBase;
                if (pageBase == null)
                    return;

                var originalName = contextNode.Name;

                //contextNode.Name = RenameTargetTextBox.Text;
                //contextNode.Save();
                this.RenameContentView.UpdateContent();
                this.RenameContentView.Content.Save();


                var back = PortalContext.Current.BackUrl;
                var oldUrlName = string.Format("/{0}", originalName);
                var newUrlName = string.Format("/{0}", this.RenameContentView.Content.Name);

                //if the user invoked the Edit action from the content itself,
                //we should redirect the response to a backurl containing the new name
                if (!string.IsNullOrEmpty(originalName) && originalName.CompareTo(this.RenameContentView.Content.Name) != 0)
                {
                    if (back.EndsWith(oldUrlName))
                    {
                        var oldIndex = back.LastIndexOf(oldUrlName);
                        back = back.Remove(oldIndex) + newUrlName;
                    }
                    else if (back.Contains(string.Concat(oldUrlName, "?")))
                    {
                        var paramsIndex = back.IndexOf("?");
                        var parameters = back.Substring(paramsIndex);
                        back = back.Remove(paramsIndex).Remove(back.LastIndexOf(oldUrlName)) + newUrlName + parameters;
                    }

                    pageBase.Response.Redirect(back, false);
                }
                else
                {
                    pageBase.Done(false);
                }
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
            var genericContent = GetContextNode() as GenericContent;
            if (genericContent == null)
                return;

            if (ContentLabel != null)
                ContentLabel.Text = genericContent.Name;

            if (RenameButton != null)
                RenameButton.Click += RenameButton_Click;
        }

        protected void SetControlVisibility(bool finished)
        {
            if (CancelButton != null)
                CancelButton.Visible = !finished;
            if (RenameButton != null)
                RenameButton.Visible = !finished;
        }

        private void HideErrorPanel()
        {
            if (ErrorPlaceholder != null)
                ErrorPlaceholder.Visible = false;
            if (ErrorLabel != null)
                ErrorLabel.Visible = false;
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
