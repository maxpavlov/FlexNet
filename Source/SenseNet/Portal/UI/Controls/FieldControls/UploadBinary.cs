using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.UI.Controls
{
    [ToolboxData("<{0}:UploadBinary ID=\"UploadBinary1\" runat=server></{0}:UploadBinary>")]
    public class UploadBinary : FieldControl, INamingContainer, ITemplateFieldControl
    {
        // Members ////////////////////////////////////////////////////////////////
        public virtual string GetControlId(Control control, Node node)
        {
            return String.Concat(control.ID, "_", node.Id.ToString());
        }
        private FileUpload _fileUploadControl;
        private Label _info;
        private BinaryData _data;

        public UploadBinary()
        {
            InnerControlID = "FileUploader";
            _fileUploadControl = new FileUpload { ID = InnerControlID };
            _info = new Label { ID = "FileUploaderInfo" };

        }

        // Events ////////////////////////////////////////////////////////////////
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            #region template

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
                return;

            #endregion

            _fileUploadControl.ID = GetControlId(_fileUploadControl, ContentHandler);
            _info.ID = GetControlId(_info, ContentHandler);
            Controls.Add(_fileUploadControl);
            Controls.Add(_info);
        }
        public override object GetData()
        {

            if (_data == null)
                _data = new BinaryData();

            if (!UseBrowseTemplate && !UseEditTemplate && !UseInlineEditTemplate)
            {
                if (!_fileUploadControl.HasFile)
                    return _data;

                SetBinaryDataProperties(_fileUploadControl, _data);
            }
            var innerControl = GetInnerControl() as FileUpload;
            if (innerControl == null)
                return _data;

            if (!innerControl.HasFile)
                return _data;

            SetBinaryDataProperties(_fileUploadControl, _data);
            
            return _data;
        }

        private static void SetBinaryDataProperties(FileUpload fileUpload, BinaryData data)
        {
            var fileStream = fileUpload.PostedFile.InputStream;
            var contentType = fileUpload.PostedFile.ContentType;
            var fileName = fileUpload.PostedFile.FileName;

            data.ContentType = contentType;
            data.FileName = fileName;
            data.SetStream(fileStream);  
        }

        public override object Data
        {
            get
            {
                var result = this.GetData() as BinaryData;
                if (result != null && result.IsEmpty)
                        return null;
                return result.FileName;
            }
        }

        public override void SetData(object data)
        {
            _data = data as BinaryData;
            if (_data != null)
                _info.Text = _data.FileName;
            
            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            
            if (title != null)
                title.Text = this.Field.DisplayName;

            if (desc != null)
                desc.Text = this.Field.Description;

            #endregion
        }


        #region ITemplateFieldControl Members

        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID) as TextBox;
        }

        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID) as Label;
        }

        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID) as Label;
        }

        #endregion
    }
}
