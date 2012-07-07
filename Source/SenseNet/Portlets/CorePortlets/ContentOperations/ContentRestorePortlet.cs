using System;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI.Controls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.ComponentModel;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public class ContentRestorePortlet : ContextBoundPortlet
    {
        //====================================================================== Constructor

        public ContentRestorePortlet()
        {
            this.Name = "Restore";
            this.Description = "This portlet restores the content from the trash (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.ContentOperation);
        }

        //====================================================================== Properties

        private string _viewPath = "/Root/System/SystemPlugins/Portlets/ContentRestore/Restore.ascx";
        private string _errorViewPath = "/Root/System/SystemPlugins/Portlets/ContentRestore/RestoreError.ascx";
        private string _infoViewPath = "/Root/System/SystemPlugins/Portlets/ContentRestore/RestoreInfo.ascx";

        [LocalizedWebDisplayName(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DISPLAYNAME)]
        [LocalizedWebDescription(PORTLETFRAMEWORK_CLASSNAME, RENDERER_DESCRIPTION)]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ViewPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ViewPath
        {
            get { return _viewPath; }
            set { _viewPath = value; }
        }

        [WebDisplayName("Error view path")]
        [WebDescription("Path of the .ascx user control which provides the UI elements for the error dialog")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(110)]
        public string ErrorViewPath
        {
            get { return _errorViewPath; }
            set { _errorViewPath = value; }
        }

        [WebDisplayName("Info view path")]
        [WebDescription("Path of the .ascx user control which provides the UI elements for the info dialog")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(120)]
        public string InfoViewPath
        {
            get { return _infoViewPath; }
            set { _infoViewPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }


        private TextBox _destinationTextBox;
        protected TextBox DestinationTextBox
        {
            get
            {
                if (_destinationTextBox == null)
                {
                    _destinationTextBox = this.FindControlRecursive("Destination") as TextBox;
                }

                return _destinationTextBox;
            }
        }

        private Button _destinationButton;
        protected Button DestinationButton
        {
            get
            {
                if (_destinationButton == null)
                {
                    _destinationButton = this.FindControlRecursive("DestinationPicker") as Button;
                }

                return _destinationButton;
            }
        }

        private Label _contentLabel;
        protected Label ContentLabel
        {
            get
            {
                if (_contentLabel == null)
                {
                    _contentLabel = this.FindControlRecursive("LabelContent") as Label;
                }

                return _contentLabel;
            }
        }

        private Label _messageLabel;
        protected Label MessageLabel
        {
            get
            {
                if (_messageLabel == null)
                {
                    _messageLabel = this.FindControlRecursive("LabelMessage") as Label;
                }

                return _messageLabel;
            }
        }

        private Label _messageDescLabel;
        protected Label MessageDescLabel
        {
            get
            {
                if (_messageDescLabel == null)
                {
                    _messageDescLabel = this.FindControlRecursive("LabelDesc") as Label;
                }

                return _messageDescLabel;
            }
        }

        private Button _newNameButton;
        protected Button NewNameButton
        {
            get
            {
                if (_newNameButton == null)
                {
                    _newNameButton = this.FindControlRecursive("NewNameBtn") as Button;
                }

                return _newNameButton;
            }
        }

        private MessageControl _msgControl;
        protected MessageControl MessageControl
        {
            get
            {
                if (_msgControl == null && this.Controls.Count > 0)
                {
                    _msgControl = this.Controls[0].FindControl("RestoreMessage") as MessageControl;
                }

                return _msgControl;
            }
        }

        protected RestoreResultType RestoreResult { get; set; }
        protected string RestoreTarget { get; set; }

        //====================================================================== Methods

        protected override void OnInit(EventArgs e)
        {
            Page.RegisterRequiresControlState(this);

            base.OnInit(e);
        }

        protected override void CreateChildControls()
        {
            if (this.RestoreResult != RestoreResultType.Nonedefined)
                BuildResultScreen(null);
            else
                BuildMainScreen();
        }

        protected override object SaveControlState()
        {
            var state = new object[3];

            state[0] = base.SaveControlState();
            state[1] = this.RestoreResult;
            state[2] = this.RestoreTarget;

            return state;
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                var state = savedState as object[];
                if (state != null && state.Length == 3)
                {
                    base.LoadControlState(state[0]);

                    this.RestoreResult = state[1] == null ? 
                        RestoreResultType.Nonedefined :
                        (RestoreResultType)state[1];

                    this.RestoreTarget = state[2] as string;
                }
            }
            else
                base.LoadControlState(savedState);
        }

        protected void BuildMainScreen()
        {
            ClearControls();

            var c = Page.LoadControl(ViewPath);
            if (c == null)
                return;

            this.Controls.Add(c);

            var trashBag = GetContextNode() as TrashBag;
            if (trashBag == null)
                return;

            if (this.MessageControl != null)
            {
                this.MessageControl.ButtonsAction += MessageControl_ButtonsAction;
            }

            if (this.DestinationTextBox != null)
            {
                this.DestinationTextBox.Text = trashBag.OriginalPath;

                if (this.DestinationButton != null)
                {
                    this.DestinationButton.OnClientClick = GetOpenContentPickerScript(this.DestinationTextBox);
                }
            }

            if (this.ContentLabel != null)
            {
                var lc = trashBag.DeletedContent;
                if (lc != null)
                    this.ContentLabel.Text = lc.DisplayName;
            }
        }

        protected void BuildResultScreen(RestoreException rex)
        {
            ClearControls();

            if (this.RestoreResult == RestoreResultType.Nonedefined)
                return;

            var trashBag = GetContextNode() as TrashBag;
            if (trashBag == null)
                return;

            var view = this.InfoViewPath;
            var messageTitle = string.Empty;
            var messageDesc = string.Empty;

            switch (this.RestoreResult)
            {
                case RestoreResultType.UnknownError:
                    view = this.ErrorViewPath;
                    break;
            }

            var c = Page.LoadControl(view);
            if (c == null)
                return;

            this.Controls.Add(c);

            if (rex != null)
            {
                //build UI info from the exception
                var folderName = string.IsNullOrEmpty(rex.ContentPath) ?
                    "{folder}" : RepositoryPath.GetFileNameSafe(RepositoryPath.GetParentPath(rex.ContentPath));
                var contentName = string.IsNullOrEmpty(rex.ContentPath) ?
                    "{content}" : RepositoryPath.GetFileNameSafe(rex.ContentPath);

                switch (rex.ResultType)
                {
                    case RestoreResultType.Nonedefined:
                    case RestoreResultType.UnknownError:
                        messageTitle = rex.Message;
                        messageDesc = rex.ToString();
                        break;
                    case RestoreResultType.PermissionError:
                        messageTitle = "Not enough permissions to complete the operation";
                        messageDesc = rex.Message;
                        break;
                    case RestoreResultType.ForbiddenContentType:
                        messageTitle = "Cannot restore this type of content to the selected target";
                        break;
                    case RestoreResultType.ExistingName:
                        messageTitle = "Content with this name already exists in the folder " + folderName;
                        messageDesc =
                            string.Format(
                                "You can restore it with a new name (<strong>{0}</strong>) or please choose a different destination",
                                contentName);
                        break;
                    case RestoreResultType.NoParent:
                        messageTitle = "Destination folder is missing";
                        messageDesc = "Please choose a different destination";
                        break;
                }
            }

            if (this.MessageControl != null)
            {
                this.MessageControl.ButtonsAction += MessageControl_ButtonsAction;

                if (this.NewNameButton != null)
                    this.NewNameButton.Command += MessageControl_ButtonsAction;

                if (this.MessageLabel != null)
                    this.MessageLabel.Text = messageTitle;

                if (this.MessageDescLabel != null)
                    this.MessageDescLabel.Text = messageDesc;
            }

            if (this.ContentLabel != null)
            {
                var lc = trashBag.DeletedContent;
                if (lc != null)
                    this.ContentLabel.Text = lc.DisplayName;
            }

            ShowHideControls();
        }

        protected void ShowHideControls()
        {
            switch (this.RestoreResult)
            {
                case RestoreResultType.NoParent:
                case RestoreResultType.PermissionError:
                case RestoreResultType.ForbiddenContentType:
                case RestoreResultType.UnknownError:
                case RestoreResultType.Nonedefined:
                    if (this.NewNameButton != null)
                        this.NewNameButton.Visible = false;
                    break;
            }
        }

        protected void ClearControls()
        {
            this.Controls.Clear();

            _contentLabel = null;
            _destinationTextBox = null;
            _destinationButton = null;
            _msgControl = null;
            _newNameButton = null;
            _messageLabel = null;
            _messageDescLabel = null;
        }

        private static string GetOpenContentPickerScript(Control inputTextBox)
        {
            var script = string.Format(@"javascript: SN.PickerApplication.open({{ MultiSelectMode: {0}, SelectedNodePath: {1}, callBack: {2} }}); return false;",
                "'none'",
                string.Format("$('#{0}').val()", inputTextBox.ClientID),
                string.Format("function(resultData) {{ if (!resultData) return; $('#{0}').val(resultData[0].Path); }}", inputTextBox.ClientID)
                );

            return script;
        }

        //====================================================================== Event handlers

        protected void MessageControl_ButtonsAction(object sender, CommandEventArgs e)
        {
            var trashBag = GetContextNode() as TrashBag;
            var target = string.Empty;

            try
            {
                switch (e.CommandName)
                {
                    case "Ok":
                    case "Cancel":
                        if (this.RestoreResult != RestoreResultType.Nonedefined)
                        {
                            //we are on the info/error page, reset 
                            //data and go back to the main screen
                            this.RestoreResult = RestoreResultType.Nonedefined;
                            this.RestoreTarget = null;

                            BuildMainScreen();
                        }
                        else
                            CallDone();
                        break;

                    case "Restore":

                        if (this.DestinationTextBox != null)
                            target = this.DestinationTextBox.Text;

                        if (trashBag != null)
                        {
                            TrashBin.Restore(trashBag, target);
                            RedirectSafely(trashBag, target);
                        }

                        break;

                    case "RestoreWithNewName":
                        if (this.RestoreResult != RestoreResultType.Nonedefined && trashBag != null)
                        {
                            //use the previously serialized target here, to try again
                            TrashBin.Restore(trashBag, this.RestoreTarget, true);
                            RedirectSafely(trashBag, this.RestoreTarget);
                        }
                        break;

                    case "TryAgain":
                        if (this.RestoreResult != RestoreResultType.Nonedefined && trashBag != null)
                        {
                            //use the previously serialized target here, to try again
                            TrashBin.Restore(trashBag, this.RestoreTarget);
                            RedirectSafely(trashBag, this.RestoreTarget);
                        }
                        break;
                }
            }
            catch (RestoreException rex)
            {
                //collect data from the exception to serialize it later
                this.RestoreResult = rex.ResultType;
                this.RestoreTarget = RepositoryPath.GetParentPath(rex.ContentPath);

                BuildResultScreen(rex);
            }
        }

        //====================================================================== Helper methods

        private void RedirectSafely(TrashBag bag, string targetPath)
        {
            if (bag == null)
            {
                CallDone();
                return;
            }

            var bagName = bag.Name;
            var back = PortalContext.Current.BackUrl;

            if (string.IsNullOrEmpty(back))
            {
                HttpContext.Current.Response.Redirect(TrashBin.TrashBinPath);
            }

            if (back.Contains(bagName))
            {
                //Redirect to the original location
                HttpContext.Current.Response.Redirect(targetPath);

                //ALTERNATIVE BEHAVIOR: redirect to the back url that is stored in the current back url...
                //if (back.Contains(PortalContext.BackUrlParamName))
                //{
                   
                    //var nextBack = back.Substring(back.IndexOf(PortalContext.BackUrlParamName + "=")).Remove(0, PortalContext.BackUrlParamName.Length + 1);
                    //if (nextBack.Contains("&"))
                    //    nextBack = nextBack.Remove(nextBack.IndexOf("&"));

                    //HttpContext.Current.Response.Redirect(HttpUtility.UrlDecode(nextBack));
                //}
                //else
                //{
                //    HttpContext.Current.Response.Redirect(TrashBin.TrashBinPath);
                //}
            }
            else
            {
                CallDone();
            }
        }
    }
}
