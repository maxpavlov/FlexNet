using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web;
using System.ComponentModel;
using System.Web.Script.Serialization;
using SenseNet.ContentRepository.Storage.ApplicationMessaging;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.Portal.Handlers;
using SenseNet.ContentRepository.Schema;
using SenseNet.Portal.Virtualization;
using System.Linq;

[assembly: WebResource("SenseNet.Portal.UI.Controls.Upload.js", "application/x-javascript")]
namespace SenseNet.Portal.UI.Controls
{
    [Designer(typeof(UploadDesigner))]
    [ToolboxData("<{0}:Upload ID=\"Uploader1\" runat=\"server\"></{0}:Upload")]
    public class Upload : WebControl, IScriptControl, INamingContainer
    {

        // Members ////////////////////////////////////////////////////////////////

        private const string ContainerSuffix = "_container";
        private const string ProgressBarSuffix = "_progressContainer";
        private const string CancelButtonSuffix = "_cancelButton";
        private const string FakeButtonSuffix = "_fakeButton";

        private ScriptManager _sm;
        protected readonly bool _isEmpty;
        private Dictionary<string,string> _post_params;
        private List<ContentType> _allowedTypes;

        // Properties /////////////////////////////////////////////////////////////
        public string file_post_name { get; set; }
        public bool use_query_string { get; set; }
        public string requeue_on_error { get; set; }
        public string http_success { get; set; }
        public bool debug { get; set; }
        public bool prevent_swf_caching { get; set; }

        public string button_placeholder_id { get; set; }
        public string button_image_url { get; set; }
        public int button_width { get; set; }
        public int button_height { get; set; }
        public string button_text { get; set; }
        public string button_text_style { get; set; }
        public string button_text_left_padding { get; set; }
        public string button_text_top_padding { get; set; }
        public string button_action { get; set; }
        public bool button_disabled { get; set; }
        public string button_cursor { get; set; }
        public string button_window_mode { get; set; }

        public Dictionary<string, string> Post_params
        {
            get
            {
                if (_post_params == null)
                    _post_params = new Dictionary<string, string>();
                return _post_params;
            }
        }
        public string UploadUrl { get; set; }
        public string FlashUrl { get; set; }     
        public string File_size_limit { get; set; }
        public string File_types { get; set; }
        public int File_upload_limit { get; set; }
        public int File_queue_limit { get; set; }
        public bool Begin_upload_on_queue { get; set; }
        public string File_types_description { get; set; }
       
        /// <summary>
        /// True, if the control has to display the contenttype list.
        /// </summary>
        public bool AllowOtherContentType { get; set; }
        /// <summary>
        /// Returns the id of the container element.
        /// </summary>
        public string GetContainerId
        {
            get { return String.Concat(this.ClientID, ContainerSuffix); }
        }
        /// <summary>
        /// Returns the id of the container element which stores the the progressbar of the the swfupload.
        /// </summary>
        private string GetProgressContainerId
        {
            get { return String.Concat(this.GetContainerId, ProgressBarSuffix); }
        }
        /// <summary>
        /// Returns the id of the cancel button.
        /// </summary>
        private string GetCancelButtonId
        {
            get { return String.Concat(this.GetContainerId, CancelButtonSuffix); }
        }
        private string GetFakeButtonId
        {
            get { return String.Concat(this.GetContainerId, FakeButtonSuffix); }
        }
        // Constructor ///////////////////////////////////////////////////////////
        public Upload()
        {
			_isEmpty = false;    // It indicates whether Repository property has Binary or not. False for basic component test.

        }

        // Events /////////////////////////////////////////////////////////////////
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            UITools.AddScript(UITools.ClientScriptConfigurations.SwfUploadPath);
            UITools.AddScript(UITools.ClientScriptConfigurations.SwfObjectPath);

            // add widgetcss: this contains z-index style settings to bring forward swf object button and push backward fake uploadbutton
            UITools.AddStyleSheetToHeader(Page.Header, UITools.ClientScriptConfigurations.SNWidgetsCss, 100);

            _allowedTypes = GetAllowedTypes();
        }
        protected override void OnPreRender(EventArgs e)
        {
            _sm = ScriptManager.GetCurrent(this.Page);
            if (_sm == null)
                throw new HttpException("Uploader control requires ScriptManager on the current page.");

            if (!this.DesignMode) 
                _sm.RegisterScriptControl(this);

            base.OnPreRender(e);
        }       
        protected override void Render(HtmlTextWriter writer)
        {
            if (!this.DesignMode) 
                _sm.RegisterScriptDescriptors(this);
          
            base.Render(writer);
        }
        public override void RenderBeginTag(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, GetContainerId);
            if (!string.IsNullOrEmpty(this.CssClass))
                writer.AddAttribute(HtmlTextWriterAttribute.Class,this.CssClass);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
        }
        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.DesignMode) return;
            
            this.RenderUploadButton(writer);
            if (AllowOtherContentType)
                RenderContentTypeDropDown(writer);
            this.RenderCancelButton(writer);
            this.RenderProgressBar(writer);
            
            if (!this._isEmpty) 
                return;
            this.RenderEmptyEntry(writer);
            this.RenderFileInfo(writer);
        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            writer.RenderEndTag();
        }        
        protected virtual void RenderContentTypeDropDown(HtmlTextWriter writer)
        {
            //writer.AddAttribute(HtmlTextWriterAttribute.Id,String.Concat(GetContainerId,"_contenttypes"));
            //writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            var contentTypesList = CreateContentTypeList();
            if (contentTypesList == null)
                return;

            var srUpladSpecificContentType = HttpContext.GetGlobalResourceObject("Portal", "UpladSpecificContentType") as string;
            writer.Write(srUpladSpecificContentType);

            contentTypesList.RenderControl(writer);
            //writer.RenderEndTag();
        }

        protected virtual void RenderEmptyEntry(HtmlTextWriter writer)
        {
            writer.Write(HttpContext.GetGlobalResourceObject("Portal", "UpladFileErrorNotUploaded") as string);
        }
        protected virtual void RenderDeleteButton(HtmlTextWriter writer)
        {
            throw new NotSupportedException("DeleteButton");
        }
        protected virtual void RenderUploadButton(HtmlTextWriter writer)
        {
            //<span id=\"spanButtonPlaceholder\"></span>
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "spanButtonPlaceholder");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.RenderEndTag();

            RenderFakeButton(writer);

        }


        protected virtual void RenderCancelButton(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetCancelButtonId);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, HttpContext.GetGlobalResourceObject("Portal", "UpladFileStopUploadButton") as string);
            writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            writer.AddAttribute(HtmlTextWriterAttribute.Onclick, String.Concat("FileUploadUtility.stopUpload('", this.GetContainerId, "')"));
            //writer.AddAttribute(HtmlTextWriterAttribute.Style, "font-size:8pt;");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-upload-cancel");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }
        protected virtual void RenderProgressBar(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetProgressContainerId);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            writer.RenderEndTag();
        }
        protected virtual void RenderFileInfo(HtmlTextWriter writer)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();
            //this.RenderDeleteButton(writer);
        }

        private void RenderFakeButton(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "button");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.GetFakeButtonId);
            writer.AddAttribute(HtmlTextWriterAttribute.Value, HttpContext.GetGlobalResourceObject("Portal", "UpladFileButton") as string);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "sn-upload-fakebtn");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
        }

        // IScriptControl members /////////////////////////////////////////////////
        protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors()
        {
            var descriptor = CreateDescriptor();
            return new[] { descriptor };
        }
        protected virtual IEnumerable<ScriptReference> GetScriptReferences()
        {
            var uploadScriptRef = CreateScriptReference();
            return new[] { uploadScriptRef };
        }
        IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors()
        {
            return this.GetScriptDescriptors();
        }
        IEnumerable<ScriptReference> IScriptControl.GetScriptReferences()
        {
            return this.GetScriptReferences();
        }

        // Internals //////////////////////////////////////////////////////////////
        private List<ContentType> GetAllowedTypes()
        {
            var parent = PortalContext.Current.ContextNode as GenericContent;
            var allowedTypes = parent.GetAllowedChildTypes();

            // check for files
            var allowedFileTypes = allowedTypes.Where(t => t.IsInstaceOfOrDerivedFrom("File")).ToList();
            return allowedFileTypes;
        }
        private Control CreateContentTypeList()
        {
            if (_allowedTypes.Count == 0)
                return null;

            var contentTypesCtlID = String.Concat(GetContainerId, "_contenttypes");

            // if only a single type is allowed, use this
            if (_allowedTypes.Count == 1)
            {
                var contentTypesListCtl = new Label { ID = contentTypesCtlID, Text = _allowedTypes[0].DisplayName };
                return contentTypesListCtl;
            }
            else
            {
                var contentTypesListCtl = new DropDownList { ID = contentTypesCtlID };
                contentTypesListCtl.Items.Add("Auto");
                foreach (var contentType in _allowedTypes)
                    contentTypesListCtl.Items.Add(new ListItem(contentType.DisplayName, contentType.Name));
                var onChangeScript = String.Format("FileUploadUtility.addContentType('{0}',this.options[this.selectedIndex].value)", GetContainerId);
                contentTypesListCtl.Attributes.Add("onchange", onChangeScript);

                return contentTypesListCtl;
            }
        }
        private ScriptReference CreateScriptReference()
        {
            var uploadUrl = this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "SenseNet.Portal.UI.Controls.Upload.js");
            return new ScriptReference
            {
                Path = uploadUrl,
                NotifyScriptLoaded = true
            }; ;
        }
        private ScriptControlDescriptor CreateDescriptor()
        {
            var descriptor = new ScriptControlDescriptor("SenseNet.Portal.UI.Controls.Upload", this.GetContainerId);

            descriptor.AddProperty("flashUrl", FlashUrl);
            descriptor.AddProperty("uploadUrl", UploadUrl);
            descriptor.AddProperty("progressTarget", GetProgressContainerId);
            descriptor.AddProperty("cancelButtonId", GetCancelButtonId);
            descriptor.AddProperty("file_size_limit", File_size_limit);
            descriptor.AddProperty("file_types", File_types);
            descriptor.AddProperty("file_types_description", File_types_description);
            descriptor.AddProperty("file_upload_limit", File_upload_limit);
            descriptor.AddProperty("file_queue_limit", File_queue_limit);
            descriptor.AddProperty("begin_upload_on_queue", Begin_upload_on_queue);

            UploadToken uploadToken = UploadToken.Generate(User.Current.Id);
            uploadToken.Persist();
            this.Post_params.Add("UploadToken", uploadToken.UploadGuid.ToString());
            var context = SenseNet.Portal.Virtualization.PortalContext.Current;
            if (context.ContextNodeHead != null)
            {
                if (!Post_params.ContainsKey("ParentId"))
                    this.Post_params.Add("ParentId",context.ContextNodeHead.Id.ToString());

                if (_allowedTypes.Count == 1 && !Post_params.ContainsKey("ContentType"))
                    this.Post_params.Add("ContentType", _allowedTypes[0].Name);
            }

            // convert Post_params to json, in 3.5 consider to use DataContractJsonSerializer
            var serializer = new JavaScriptSerializer();
            var sb = new StringBuilder();
            serializer.Serialize(this.Post_params, sb);
            descriptor.AddProperty("post_params", sb.ToString());

            descriptor.AddProperty("file_post_name", file_post_name);
            descriptor.AddProperty("use_query_string", use_query_string);
            descriptor.AddProperty("requeue_on_error", requeue_on_error);
            descriptor.AddProperty("http_success", http_success);
            descriptor.AddProperty("debug", debug);
            descriptor.AddProperty("prevent_swf_caching", prevent_swf_caching);
            descriptor.AddProperty("button_placeholder_id", button_placeholder_id);
            descriptor.AddProperty("button_image_url", button_image_url);
            descriptor.AddProperty("button_width", button_width);
            descriptor.AddProperty("button_height", button_height);
            descriptor.AddProperty("button_text", button_text);
            descriptor.AddProperty("button_text_style", button_text_style);
            descriptor.AddProperty("button_text_left_padding", button_text_left_padding);
            descriptor.AddProperty("button_text_top_padding", button_text_top_padding);
            descriptor.AddProperty("button_action", button_action);
            descriptor.AddProperty("button_disabled", button_disabled);
            descriptor.AddProperty("button_cursor", button_cursor);
            descriptor.AddProperty("button_window_mode", button_window_mode);

            return descriptor;
        }
    }

    public class UploadDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            return base.GetDesignTimeHtml() + base.CreatePlaceHolderDesignTimeHtml();
        }
    }
}
