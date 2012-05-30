using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;


using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

using SN = SenseNet.ContentRepository;
using SNP = SenseNet.Portal;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;


namespace SenseNet.Portal.UI.Controls
{
    public enum BinaryEditorMode
    {
        Binary = 0, Text
    }

    [ToolboxData("<{0}:Binary ID=\"Binary1\" runat=server></{0}:Binary>")]
    public class Binary : FieldControl, INamingContainer, ITemplateFieldControl
    {
        // Members ////////////////////////////////////////////////////////////////

        private BinaryData _data;
        private Control _control;
        private readonly TextBox _textBox;

        private FileUpload _fileUploadControl;

        // Properties /////////////////////////////////////////////////////////////
        public bool AutoName { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public BinaryEditorMode Mode { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public int Rows { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public int Columns { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public string ValidTypes { get; set; }
        [PersistenceMode(PersistenceMode.Attribute)]
        public bool FullScreenText { get; set; }

        public override OutputMethod DefaultOutputMethod { get { return ContentRepository.Schema.OutputMethod.Raw; } }

        // Constructor ////////////////////////////////////////////////////////////
        /// <summary>
        /// Initializes a new instance of the <see cref="Binary"/> class.
        /// </summary>
        public Binary()
        {

                InnerControlID = "BinaryTextBox";

            _textBox = new TextBox { ID = InnerControlID };
            _fileUploadControl = new FileUpload { ID = "FileUploader" };

            Mode = BinaryEditorMode.Text;
            AutoName = true;
        }

        // Events /////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets object data.
        /// </summary>
        /// <returns>
        /// Object representing data of the wrapped Field
        /// </returns>
        /// <remarks>
        /// Exception handling and displayed is done at ContentView level; FormatExceptions and Exceptions are handled and displayed at this level.
        /// Should you need custom or localized error messages, throw a FieldControlDataException with your own error message.
        /// </remarks>
        public override object GetData()
        {
            if (_data == null)
                _data = new BinaryData();

            #region template

            
            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
            {
                SetBinaryDataProperties(_data, _fileUploadControl, _textBox);

                return _data;
            }

            SetBinaryDataProperties(_data, GetUploadeControl() as FileUpload, GetInnerControl() as ITextControl);

            return _data;

            #endregion
        }

        private void SetBinaryDataProperties(BinaryData data, FileUpload fileUpload, ITextControl editor)
        {
            //if (editor == null) 
            //    return;
            //var hasText = editor.Text.Length > 0;
            //if (fileUpload != null && fileUpload.HasFile)
            //{
            //    var fileStream = fileUpload.PostedFile.InputStream;
            //    var contentType = fileUpload.PostedFile.ContentType;
            //    var fileName = fileUpload.PostedFile.FileName;

            //    data.ContentType = contentType;
            //    data.FileName = fileName;
            //    data.SetStream(fileStream);

            //    if (AutoName)
            //        Content.Fields["Name"].SetData(Path.GetFileName(fileName));
            //}
            //else if (hasText)
            //    data.SetStream(Tools.GetStreamFromString(editor.Text));

            if (editor == null)
                return;
            var textMode = ((TextBox)editor).Visible;
            if (textMode)
            {
                data.SetStream(Tools.GetStreamFromString(editor.Text));
            }
            else
            {
                if (fileUpload != null && fileUpload.HasFile)
                {
                    var fileStream = fileUpload.PostedFile.InputStream;
                    var contentType = fileUpload.PostedFile.ContentType;
                    var fileName = fileUpload.PostedFile.FileName;

                    data.ContentType = contentType;
                    data.FileName = fileName;
                    data.SetStream(fileStream);

                    //if (AutoName)
                    //    Content.Fields["Name"].SetData(Path.GetFileName(fileName));
                }
            }
        }

        /// <summary>
        /// Sets data within the FieldControl
        /// </summary>
        /// <param name="data">Data of the <see cref="SenseNet.ContentRepository.Field">Field</see> wrapped</param>
        public override void SetData(object data)
        {
            _data = data as BinaryData;
            var stream = _data == null ? null : _data.GetStream();
            var streamString = string.Empty;

            try
            {
                if (IsTextInternal && stream != null)
                    streamString = Tools.GetStreamString(stream);
            }
            catch (Exception ex)
            {
                // failed to load stream
                // streamString will be string.Empty, no need for special actions
                Logger.WriteException(ex);
            }

            if (_textBox != null)
                _textBox.Text = streamString;

            #region template

            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;

            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            var innerControl = GetInnerControl() as TextBox;
            if (title != null)
                title.Text = this.Field.DisplayName;
            if (desc != null)
                desc.Text = this.Field.Description;
            if (innerControl != null)
                innerControl.Text = streamString;

            #endregion
        }
        /// <summary>
        /// Raises the <see cref="E:Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            UITools.AddScript(UITools.ClientScriptConfigurations.SNBinaryFieldControlPath);
            
            base.OnInit(e);

            if (UseBrowseTemplate || UseEditTemplate || UseInlineEditTemplate)
            {
                var t = GetInnerControl() as TextBox;
                var f = GetUploadeControl() as FileUpload;
                if (t!=null)
                    t.Visible = IsTextInternal;
                if (f!=null)
                    f.Visible = IsTextInternal == false;

                return;
            }

            _textBox.TextMode = TextBoxMode.MultiLine;
            _textBox.Rows = (Rows == 0) ? 5 : this.Rows;
            _textBox.Columns = (Columns == 0) ? 20 : this.Columns;
            _textBox.CssClass = String.IsNullOrEmpty(this.CssClass) ? "" : this.CssClass;
            Controls.Add(_textBox);
            Controls.Add(_fileUploadControl);
            // don't move this property setters before the base.OnInit call due to
            _textBox.Visible = IsTextInternal;
            _fileUploadControl.Visible = IsTextInternal == false;

            if (this.FullScreenText)
                _fileUploadControl.Visible = false;
        }
        protected override void InitTemplates()
        {
            // if fullscreentext is set, show no frames for texts. show simple binary upload otherwise, with the set framemode
            if (FullScreenText && IsTextInternal)
                this.FrameMode = FieldControlFrameMode.NoFrame;

            base.InitTemplates();
        }
        /// <summary>
        /// Renders the contents of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        protected override void RenderContents(HtmlTextWriter writer)
        {

            if (UseBrowseTemplate)
            {
                base.RenderContents(writer);
                return;
            }

            if (UseEditTemplate)
            {
                base.RenderContents(writer);
                return;
            }

            if (UseInlineEditTemplate)
            {
                base.RenderContents(writer);
                return;
            }

            if (RenderMode == FieldControlRenderMode.Browse)
            {
                RenderHtmlAnchor(writer);
                return;
            }
            RenderEditor(writer);
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (!this.FullScreenText)
                UITools.RegisterStartupScript("initzoom", "SN.BinaryFieldControl.initZoomWindow();", this.Page);
            else
            {
                var extension = string.Empty;
                if (ContentHandler != null)
                    extension = Path.GetExtension(ContentHandler.Path).ToLower();
                UITools.RegisterStartupScript("inithighlight",
                                              string.Format("SN.BinaryFieldControl.initHighlightTextbox('{0}');",
                                                            extension), this.Page);
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Renders the HTML anchor.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderHtmlAnchor(HtmlTextWriter writer)
        {
            var htmlLink = new System.Web.UI.HtmlControls.HtmlAnchor
            {
                HRef = this.Field.Content.Path,
                Target = "_blank",
                InnerText = this.Field.Content.Name
            };
            htmlLink.RenderControl(writer);
        }
        /// <summary>
        /// Gets the control id.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected virtual string GetControlId(Control control, Node node)
        {
            return String.Concat(control.ID, "_", node.Id.ToString());
        }
        /// <summary>
        /// Gets a value indicating whether this instance is text internal.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is text internal; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool IsTextInternal
        {
            get
            {
                // this code (surrounded with region) is needed for backward compatibility

                #region

                //  these are special use cases because ContentType is not derived from File contenttype that's why it
                //  hasn't got Binary property, the rest is 
                var isContentType = ContentHandler is ContentType;
                if (isContentType)
                    return true;

                // we don't have decision function (yet) whether stream of the binary field contains readable bytes or not.
                var extension = Path.GetExtension(ContentHandler.Path);
                if (!String.IsNullOrEmpty(extension))
                    return Repository.EditSourceExtensions.Contains(extension);

                #endregion

                var settings = (BinaryFieldSetting)Field.FieldSetting;
                if (settings != null)
                {
                    var isText = settings.IsText;
                    return isText.HasValue ? isText.GetValueOrDefault() : false;
                }
                return false;
            }
        }
        /// <summary>
        /// Renders the editor.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderEditor(HtmlTextWriter writer)
        {
            var renderMode = RenderMode;
            if (renderMode == FieldControlRenderMode.InlineEdit)
            {
                var altText = String.Concat(Field.DisplayName, " ", Field.Description);
                _textBox.Attributes.Add("Title", altText);
            }

            if (Field.ReadOnly)
                writer.Write(_textBox.Text);
            else if (ReadOnly)
            {
                // render readonly control
                _textBox.Enabled = !ReadOnly;
                _textBox.EnableViewState = false;
                RenderInnerControls(writer);
            }
            else
                RenderInnerControls(writer);


            if (IsTextInternal)
                return;

            if (ContentHandler.Id == 0)
                return;

            new LiteralControl("<br />").RenderControl(writer);
            RenderHtmlAnchor(writer);
        }

        // Internals //////////////////////////////////////////////////////////////
        /// <summary>
        /// Renders the inner controls.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderInnerControls(HtmlTextWriter writer)
        {
            if (_textBox.Visible)
                _textBox.RenderControl(writer);
            if (_fileUploadControl.Visible)
                _fileUploadControl.RenderControl(writer);
        }

        #region ITemplateFieldControl Members


        public Control GetUploadeControl()
        {
            return this.FindControlRecursive("FileUploader");
        }

        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }

        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID);
        }

        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID);
        }

        #endregion
    }
}
