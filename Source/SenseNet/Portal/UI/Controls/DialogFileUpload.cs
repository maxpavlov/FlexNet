using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;
using SenseNet.Portal.Handlers;
using System.Web;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.Portal.UI.Controls
{
    [Serializable]
    internal class FileListElement
    {
        public string Path { get; set; }
        public string FileName { get; set; }
    }

    public class DialogFileUpload : UserControl
    {
        // ==================================================================================== Properties
        public string AllowedContentTypes { get; set; }

        
        // ==================================================================================== Members
        private List<FileListElement> _fileListElements;
        private List<ContentType> AllowedContentTypesList
        {
            get
            {
                if (string.IsNullOrEmpty(AllowedContentTypes))
                    return null;
                else
                    return AllowedContentTypes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(a=>ContentType.GetByName(a)).ToList();
            }
        }


        // ==================================================================================== Controls
        private FileUpload Upload
        {
            get
            {
                return this.FindControlRecursive("Upload") as FileUpload;
            }
        }
        private Repeater UploadedFiles
        {
            get
            {
                return this.FindControlRecursive("UploadedFiles") as Repeater;
            }
        }
        private PlaceHolder ErrorPlaceHolder
        {
            get
            {
                return this.FindControlRecursive("ErrorPlaceHolder") as PlaceHolder;
            }
        }
        private Label ErrorLabel
        {
            get
            {
                return this.FindControlRecursive("ErrorLabel") as Label;
            }
        }


        // ==================================================================================== Methods
        protected override void LoadControlState(object savedState)
        {
            var state = (object[])savedState;
            _fileListElements = state[0] as List<FileListElement>;
            base.LoadControlState(state[1]);
        }
        protected override object SaveControlState()
        {
            return new object[] {
                _fileListElements,
                base.SaveControlState()
            };
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Page.RegisterRequiresControlState(this);
        }
        protected override void CreateChildControls()
        {
            ErrorPlaceHolder.Visible = false;

            if (Upload.HasFile)
            {
                try
                {
                    this.SaveFile();
                }
                catch (Exception ex)
                {
                    ErrorLabel.Text = ex.Message;
                    ErrorPlaceHolder.Visible = true;
                }
            }

            UploadedFiles.DataSource = _fileListElements;
            UploadedFiles.ItemDataBound += new RepeaterItemEventHandler(UploadedFiles_ItemDataBound);
            UploadedFiles.DataBind();
        }
        private void SaveFile()
        {
            // get target container
            var container = PortalContext.Current.ContextNode as GenericContent;
            var targetFolderName = HttpContext.Current.Request["TargetFolder"];
            if (!string.IsNullOrEmpty(targetFolderName))
            {
                var containerPath = RepositoryPath.Combine(container.Path, targetFolderName);
                container = Node.LoadNode(containerPath) as GenericContent;

                // create target container if does not exist
                if (container == null)
                {
                    using (new SystemAccount())
                    {
                        container = new Folder(PortalContext.Current.ContextNode);
                        container.Name = targetFolderName;
                        container.AllowedChildTypes = AllowedContentTypesList;
                        container.Save();
                    }
                }
            }

            var contentTypeName = UploadHelper.GetContentType(Upload.PostedFile.FileName, null) ?? "File";
            var allowed = UploadHelper.CheckAllowedContentType(container as GenericContent, contentTypeName);
            if (allowed)
            {
                var binaryData = UploadHelper.CreateBinaryData(Upload.PostedFile);
                var fileName = binaryData.FileName.ToString();

                var content = SenseNet.ContentRepository.Content.CreateNew(contentTypeName, container, fileName);
                content.ContentHandler.AllowIncrementalNaming = true;   // uploaded files of users go to the same folder. avoid collision, do not overwrite files of each other
                content["Name"] = fileName;
                content.Fields["Binary"].SetData(binaryData);
                content.Save();

                // display uploaded file in repeater
                if (_fileListElements == null)
                    _fileListElements = new List<FileListElement>();

                _fileListElements.Add(new FileListElement { FileName = content.Name, Path = content.Path });
            }
            else
            {
                ErrorLabel.Text = "This type cannot be uploaded!";
                ErrorPlaceHolder.Visible = true;
            }
        }
        void UploadedFiles_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var button = e.Item.FindControlRecursive("DeleteFile") as Button;
            if (button != null)
                button.Click += new EventHandler(DeleteFileButton_Click);
        }
        void DeleteFileButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            var path = button.CommandName;

            var node = Node.LoadNode(path);
            if (node == null)
            {
                ErrorLabel.Text = "Cannot find content!";
                ErrorPlaceHolder.Visible = true;
                return;
            }

            // check: was this file uploaded by me? if yes, delete. Otherwise don't allow delete.
            if (node.CreatedById == User.Current.Id)
            {
                Node.ForceDelete(path);

                _fileListElements = _fileListElements.Where(a => a.Path != path).ToList();
                if (_fileListElements.Count == 0)
                    _fileListElements = null;

                UploadedFiles.DataSource = _fileListElements;
                UploadedFiles.DataBind();
            }
            else
            {
                ErrorLabel.Text = "This content cannot be deleted since it was created by another user!";
                ErrorPlaceHolder.Visible = true;
            }
        }
    }
}
