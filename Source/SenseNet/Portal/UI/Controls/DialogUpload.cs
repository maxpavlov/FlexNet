using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using SenseNet.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.Portal.UI.Controls
{
    public class DialogUpload : UserControl
    {
        // ==================================================================================== Properties
        public string TargetFolderName { get; set; }
        public string UploadTypes { get; set; }
        public string ContextInfoID { get; set; }
        public string ButtonText { get; set; }
        public int DateLimitMinutes { get; set; }

        
		// ==================================================================================== Members
        private DateTime _startUploadDate { get; set; }
        private const string UPLOADCONTROLPATH = "/Root/System/SystemPlugins/Controls/DialogUpload.ascx";
        private Control UploadControl;
        private List<ContentType> UploadContentTypes
        {
            get
            {
                if (string.IsNullOrEmpty(UploadTypes))
                    return null;

                var typesList = UploadTypes.Split(',', ';');
                return typesList.Select(t => ContentType.GetByName(t)).ToList();
            }
        }


        // ==================================================================================== Controls
        private const string CONTAINERID = "Container";
        private Panel Container
        {
            get
            {
                return UploadControl.FindControlRecursive(CONTAINERID) as Panel;
            }
        }

        private const string UPLOADPATHID = "UploadPath";
        private TextBox  UploadPathControl
        {
            get
            {
                return UploadControl.FindControlRecursive(UPLOADPATHID) as TextBox;
            }
        }

        private const string TARGETFOLDERID = "TargetFolder";
        private TextBox TargetFolderControl
        {
            get
            {
                return UploadControl.FindControlRecursive(TARGETFOLDERID) as TextBox;
            }
        }


        private const string UPLOADBUTTONID = "UploadButton";
        private Button UploadButtonControl
        {
            get
            {
                return UploadControl.FindControlRecursive(UPLOADBUTTONID) as Button;
            }
        }

        private const string STARTUPLOADDATEID = "StartUploadDate";
        private TextBox StartUploadDateControl
        {
            get
            {
                return UploadControl.FindControlRecursive(STARTUPLOADDATEID) as TextBox;
            }
        }

        private const string UPLOADDIALOGID = "UploadDialog";
        private Panel UploadDialog
        {
            get
            {
                return UploadControl.FindControlRecursive(UPLOADDIALOGID) as Panel;
            }
        }

        private const string CLIENTIDCONTROLID = "ClientIdControl";
        private TextBox ClientIdControl
        {
            get
            {
                return UploadControl.FindControlRecursive(CLIENTIDCONTROLID) as TextBox;
            }
        }


        private const string DIALOGCLIENTIDCONTROLID = "DialogClientIdControl";
        private TextBox DialogClientIdControl
        {
            get
            {
                return UploadControl.FindControlRecursive(DIALOGCLIENTIDCONTROLID) as TextBox;
            }
        }


        // ==================================================================================== Methods
        protected override void LoadControlState(object savedState)
        {
            var state = (object[])savedState;
            if (state == null)
                return;

            _startUploadDate = (DateTime)state[0];
            base.LoadControlState(state[1]);
        }
        protected override object SaveControlState()
        {
            return new object[] { _startUploadDate, base.SaveControlState() };
        }
        protected override void OnInit(EventArgs e)
        {
            UploadControl = this.Page.LoadControl(UPLOADCONTROLPATH);
            this.Controls.Add(UploadControl);

            UITools.AddScript("$skin/scripts/sn/SN.DialogUpload.js");
            UITools.RegisterStartupScript("DialogUploadScript" + this.ClientID, "SN.DialogUpload.loadUploadedFiles(\"" + this.ClientID + "\");", this.Page);
            
            base.OnInit(e);
        }
        protected override void CreateChildControls()
        {
            var uploadDialog = this.UploadDialog;
            if (uploadDialog != null)
                uploadDialog.CssClass = "sn-du-uploaddialog sn-du-uploaddialog-" + this.ClientID;

            var container = this.Container;
            if (container != null)
                container.CssClass = "sn-du-container sn-du-container-" + this.ClientID;

            var clientIdControl = this.ClientIdControl;
            if (clientIdControl != null)
                clientIdControl.Text = this.ClientID;

            var dialogClientIdControl = this.DialogClientIdControl;
            if (dialogClientIdControl != null)
                dialogClientIdControl.Text = this.ClientID;

            // set the current date for checking user's upload list - but do not set it at postbacks, therefore it is stored in controlstate
            if (this._startUploadDate == DateTime.MinValue && DateLimitMinutes != 0)
            {
                this._startUploadDate = DateTime.Now.AddMinutes(-DateLimitMinutes);
            }

            var startUploadDateControl = this.StartUploadDateControl;
            if (startUploadDateControl != null)
            {
                startUploadDateControl.Text = this._startUploadDate.ToString();
            }

            var uploadButtonControl = this.UploadButtonControl;
            if (!string.IsNullOrEmpty(this.ButtonText) && uploadButtonControl != null)
            {
                uploadButtonControl.Text = this.ButtonText;
            }

            var uploadPathControl = this.UploadPathControl;
            if (uploadPathControl != null)
            {
                string containerPath = string.Empty;

                // if a contextinfo is present, it controls the context
                var contextInfo = UITools.FindContextInfo(this, ContextInfoID);
                if (contextInfo != null)
                {
                    containerPath = contextInfo.Path;
                }
                else
                {
                    // in add scenario, the content does not exist, so container for Uploads folder should be the parent.
                    // set containerpath to parent, to use consistent containers for both new and existing content
                    var contentView = UITools.FindFirstContainerOfType<ContentView>(this);
                    var contextNode = contentView.Content.ContentHandler;
                    containerPath = contextNode.ParentPath;
                }

                uploadPathControl.Text = containerPath;

                var targetFolderControl = this.TargetFolderControl;
                if (targetFolderControl != null)
                {
                    targetFolderControl.Text = this.TargetFolderName;
                }
            }

            this.ChildControlsCreated = true;
            base.CreateChildControls();
        }
    }
}
