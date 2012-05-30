using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository;


namespace SenseNet.Portal.UI.Controls
{
    /// <summary>
    /// Edit mode:
    ///  - no templates:
    ///        <sn:Image ID="Image2" runat="server" FieldName="Image" />
    ///        
    ///  - templated:
    ///        <sn:Image ID="Image" runat="server" FieldName="Image">
    ///            <EditTemplate>
    ///                <asp:Image ID="ImageControl" runat="server" class="sn-ctrl-image" />
    ///                <asp:FileUpload ID="FileUploadControl" runat="server" class="sn-ctrl-image-upload" />
    ///                <asp:CheckBox ID="ImageIsReferenceControl" runat="server" />
    ///            </EditTemplate>
    ///        </sn:Image>
    ///        
    /// Browse mode:
    ///  - no templates
    ///        <sn:Image ID="Image2" runat="server" FieldName="Image" RenderMode="Browse" />
    ///    or:
    ///        <img src="<%= "/binaryhandler.ashx?nodeid="+GetValue("Id")+"&propertyname=Image" %>"></img>
    ///        
    ///  - templated
    ///        <sn:Image ID="Image3" runat="server" FieldName="Image" RenderMode="Browse">
    ///            <BrowseTemplate>
    ///                <asp:Image ID="ImageControl" runat="server" />
    ///            </BrowseTemplate>
    ///        </sn:Image>
    ///        
    /// </summary>


    [ToolboxData("<{0}:Image ID=\"Image1\" runat=server></{0}:Image>")]
    public class Image : FieldControl, ITemplateFieldControl
    {
        /* ============================================================================= Constants */
        private const string _imageUpdateScript =
            @"
                $('.sn-ctrl-image-upload').change(function() {
                    var fileArray = document.getElementById($(this).attr('id')).files;
                    if (fileArray) {
                        newSrc = fileArray[0].getAsDataURL();
                        $('.sn-ctrl-image', $(this).parent()).attr({ src: newSrc });
                    }
                });
            ";


        /* ============================================================================= Members */
        private SenseNet.ContentRepository.Fields.ImageField.ImageFieldData _data;
        private CheckBox _cbxImageRef;
        private FileUpload _fileUploadControl;
        private System.Web.UI.WebControls.Image _imageControl;
        
        protected string ImageControlID;
        protected string ImageIsReferenceControlID;
        private BinaryData _binaryData;


        /* ============================================================================= Properties */
        public SenseNet.ContentRepository.Fields.ImageField ImageField
        {
            get
            {
                return this.Field as SenseNet.ContentRepository.Fields.ImageField;
            }
        }
        public string ImageUrl
        {
            get
            {
                string imageUrl = ImageField.ImageUrl;

                if (!string.IsNullOrEmpty(imageUrl))
                    imageUrl = string.Concat(imageUrl, SizeUrlParams);

                return imageUrl;
            }
        }
        public string SizeUrlParams
        {
            get
            {
                return
                    ContentRepository.Fields.ImageField.GetSizeUrlParams(this.ImageField.ImageMode, this.UrlWidth, this.UrlHeight);
            }
        }

        [PersistenceMode(PersistenceMode.Attribute)]
        public new int Width { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public new int Height { get; set; }
        public int UrlWidth
        {
            get
            {
                // sn:Image width given? -> that should be the width in request url
                // otherwise it's the template-control's (asp:Image) width
                if (this.Width != 0)
                    return this.Width;

                if (IsTemplated)
                {
                    var imgControl = GetImageControl();
                    if (imgControl != null)
                        return (int)imgControl.Width.Value;
                }

                return 0;
            }
        }
        public int UrlHeight
        {
            get
            {
                // sn:Image height given? -> that should be the height in request url
                // otherwise it's the template-control's (asp:Image) height
                if (this.Height != 0)
                    return this.Height;

                if (IsTemplated)
                {
                    var imgControl = GetImageControl();
                    if (imgControl != null)
                        return (int)imgControl.Height.Value;
                }

                return 0;
            }
        }


        /* ============================================================================= Constructor */
        public Image()
        {
            InnerControlID = "FileUploadControl";
            ImageControlID = "ImageControl";
            ImageIsReferenceControlID = "ImageIsReferenceControl";
            _cbxImageRef = new CheckBox { ID = ImageIsReferenceControlID };
            _fileUploadControl = new FileUpload { ID = InnerControlID, CssClass = "sn-ctrl-image-upload" };
            _imageControl = new System.Web.UI.WebControls.Image { ID = ImageControlID, CssClass = "sn-ctrl-image" };
        }


        /* ============================================================================= Methods */
        public override object GetData()
        {
            if (_data == null)
                _data = new SenseNet.ContentRepository.Fields.ImageField.ImageFieldData();

            var innerControl = _fileUploadControl;
            var imageIsRefControl = _cbxImageRef;

            // templates
            if (IsTemplated)
            {
                innerControl = GetInnerControl() as FileUpload;
                imageIsRefControl = GetImageIsRefControl() as CheckBox;
            }

            _binaryData = null;

            // newly posted filestream
            bool newStream = false;
            if (innerControl != null && innerControl.HasFile)
            {
                var fileStream = innerControl.PostedFile.InputStream;
                var contentType = innerControl.PostedFile.ContentType;
                var fileName = innerControl.PostedFile.FileName;

                _binaryData = new BinaryData();
                _binaryData.ContentType = contentType;
                _binaryData.FileName = fileName;
                _binaryData.SetStream(fileStream);

                newStream = true;
            }

            // new image mode
            var originalImageMode = this.ImageField.ImageMode;
            var newImageMode = imageIsRefControl.Checked ? ImageRequestMode.Reference : ImageRequestMode.BinaryData;

            if (!newStream)
            {
                switch (this.ImageField.ImageMode)
                {
                    case ImageRequestMode.BinaryData:
                        _binaryData = _data.ImgData;
                        break;
                    case ImageRequestMode.Reference:
                        if (_data.ImgRef != null)
                            _binaryData = _data.ImgRef.Binary;
                        break;
                }
            }

            // no uploads and no original data, so return with empty data
            if (_binaryData == null)
                return _data;

            // if mode is not changed, proceed only if new uploaded stream is available
            if ((newImageMode == this.ImageField.ImageMode) && (!newStream))
                return _data;

            // from here either mode is changed or new stream is available
            // 2 possibilities: new mode is reference or new mode is binary
            // - reference
            //    - former binarydata is cleared
            //    - the referenced node is created or updated 
            // - binary
            //    - binarydata property is set
            //    - referenced node is deleted
            if (newImageMode == ImageRequestMode.Reference)
            {
                // clear binarydata
                _data.ImgData = null;

                if (this.Content.Id != 0)
                    CreateImageReference(this.ContentHandler);
                else
                    this.ContentHandler.Created += new EventHandler<SenseNet.ContentRepository.Storage.Events.NodeEventArgs>(ContentHandler_Created);
            }
            else
            {
                // set binarydata
                _data.ImgData = new BinaryData();
                _data.ImgData.CopyFrom(_binaryData);

                // if copied from referenced node -> node name should be filename, not node's binary's filename (latter could contain '\'-s)
                if (!newStream)
                    _data.ImgData.FileName = new BinaryFileName(_data.ImgRef.Name);

                // clear referencedata (also delete the file but only after this node is saved!)
                this.ContentHandler.Modified += new EventHandler<SenseNet.ContentRepository.Storage.Events.NodeEventArgs>(ContentHandler_Modified);
            }

            // reset image url after new image is saved
            var imageControl = GetImageControl();
            if (imageControl != null)
            {
                if (!string.IsNullOrEmpty(this.ImageUrl))
                    imageControl.ImageUrl = this.ImageUrl;
            }
            
            return _data;
        }
        private void CreateImageReference(Node container)
        {
            // set referencedata (+create node with title)
            // create new node if reference not yet exists
            if (_data.ImgRef == null)
                _data.ImgRef = new SenseNet.ContentRepository.Image(this.ContentHandler);

            var content = SenseNet.ContentRepository.Content.Create(_data.ImgRef);

            // ensure that filename does not contain '\'-s
            var newName = _binaryData.FileName.ToString();
            var lastSeparator = newName.LastIndexOf('\\');
            if (lastSeparator >= 0)
                newName = newName.Substring(lastSeparator + 1);

            // also ensure that name is unique
            newName = ContentNamingHelper.EnsureContentName(newName, container);

            content["Name"] = newName;
            content["DisplayName"] = newName;
            _data.ImgRef.Binary = new BinaryData();
            _data.ImgRef.Binary.CopyFrom(_binaryData);
            content.Save();            
        }
        protected void ContentHandler_Created(object sender, SenseNet.ContentRepository.Storage.Events.NodeEventArgs e)
        {
            // reload content with image field, as it may have changed (_copying member)
            var node = Node.LoadNode(this.Content.Id);

            CreateImageReference(node);

            // set reference to content
            var content = ContentRepository.Content.Create(node);
            content[this.Field.Name] = _data;
            
            content.Save();
        }
        protected void ContentHandler_Modified(object sender, SenseNet.ContentRepository.Storage.Events.NodeEventArgs e)
        {
            if (_data.ImgRef != null)
            {
                _data.ImgRef.Delete(true);
                _data.ImgRef = null;
            }
        }
        public override void SetData(object data)
        {
            _data = data as SenseNet.ContentRepository.Fields.ImageField.ImageFieldData;

            if (!IsTemplated)
            {
                _cbxImageRef.Checked = (this.ImageField.ImageMode != ImageRequestMode.BinaryData) &&
                        (this.Field.Content.Id != 0);
                if (!string.IsNullOrEmpty(this.ImageUrl))
                    _imageControl.ImageUrl = this.ImageUrl;
                _imageControl.Width = this.Width;
                _imageControl.Height = this.Height;
            }
            else
            {
                // synchronize data with controls are given in the template
                var title = GetLabelForTitleControl() as Label;
                var desc = GetLabelForDescription() as Label;
                if (title != null)
                    title.Text = this.Field.DisplayName;
                if (desc != null)
                    desc.Text = this.Field.Description;
                var imageIsRefControl = GetImageIsRefControl();
                if (imageIsRefControl != null)
                    imageIsRefControl.Checked = (this.ImageField.ImageMode != ImageRequestMode.BinaryData) &&
                            (this.Field.Content.Id != 0);
                var imageControl = GetImageControl();
                if (imageControl != null)
                {
                    if (!string.IsNullOrEmpty(this.ImageUrl))
                        imageControl.ImageUrl = this.ImageUrl;
                }
            }
        }
        protected override void OnInit(EventArgs e)
        {
            // on picking new file, image is updated automatically (only in firefox)
            UITools.RegisterStartupScript("imageupdateonupload", _imageUpdateScript, this.Page);

            base.OnInit(e);

            if (!IsTemplated)
            {
                // original flow
                Controls.Add(_cbxImageRef);
                Controls.Add(_imageControl);
                Controls.Add(_fileUploadControl);
            }
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            // templates
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
            
            // original flow
            if (RenderMode == FieldControlRenderMode.Browse)
                RenderSimple(writer);
            else
                RenderEditor(writer);
        }
        protected virtual void RenderSimple(HtmlTextWriter writer)
        {
            _imageControl.RenderControl(writer);
        }
        protected virtual void RenderEditor(HtmlTextWriter writer)
        {
            _cbxImageRef.RenderControl(writer);
            _imageControl.RenderControl(writer);
            _fileUploadControl.RenderControl(writer);
        }
        private void ManipulateTemplateControls()
        {
            //
            //  This method is needed to ensure the common fieldcontrol logic.
            //
            var innerControl = GetInnerControl() as FileUpload;
            var imageControl = GetImageControl();
            var imageIsRefControl = GetImageIsRefControl();
            var lt = GetLabelForTitleControl() as Label;
            var ld = GetLabelForDescription() as Label;

            if (Field.ReadOnly || ReadOnly)
            {
                if (innerControl == null) 
                    return;

                var p = innerControl.Parent;
                if (p != null)
                {
                    p.Controls.Remove(innerControl);
                    p.Controls.Remove(imageIsRefControl);
                    if (lt != null) lt.AssociatedControlID = string.Empty;
                    if (ld != null) ld.AssociatedControlID = string.Empty;
                }
            }
        }


        /* ============================================================================= ITemplateFieldControl */
        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }
        public System.Web.UI.WebControls.Image GetImageControl()
        {
            return this.FindControlRecursive(ImageControlID) as System.Web.UI.WebControls.Image;
        }
        public CheckBox GetImageIsRefControl()
        {
            return this.FindControlRecursive(ImageIsReferenceControlID) as CheckBox;
        }
        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID);
        }
        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID);
        }
    }

}
