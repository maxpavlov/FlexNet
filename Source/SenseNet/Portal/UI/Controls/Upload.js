Type.registerNamespace('SenseNet.Portal.UI.Controls.Upload');

SenseNet.Portal.UI.Controls.Upload = function(element){
    SenseNet.Portal.UI.Controls.Upload.initializeBase(this, [element]);
    
    this._swfUploader = null;
    this._swfConfig = null;
    
    this._fileProgressWrapper = null;
    this._fileProgressElement = null;
    this._fileProgressHeight = 100;
    
    this._post_params = null;
    this._flashUrl = null;
    this._uploadUrl = null;
    this._progressTarget = null;
    this._cancelButtonId = null;
    this._post_params = null;
    this._file_size_limit = null;
    this._file_types = null;
    this._file_types_description = null;
    this._file_upload_limit = null;
    this._file_queue_limit = null;
    this._begin_upload_on_queue = null;
    
    
    this._file_post_name = null;
    this._use_query_string = null;
    this._requeue_on_error = null;
    this._http_success = null;
    this._debug = null;
    this._prevent_swf_caching = null;
    this._button_placeholder_id = null;
    this._button_image_url = null;
    this._button_width = null;
    this._button_height = null;
    this._button_text = null;
    this._button_text_style = null;
    this._button_text_left_padding = null;
    this._button_text_top_padding = null;
    this._button_action = null;
    this._button_disabled = null;
    this._button_cursor = null;
    this._button_window_mode = null;
    
    
}

SenseNet.Portal.UI.Controls.Upload.prototype = {

    initialize: function(element){
        SenseNet.Portal.UI.Controls.Upload.callBaseMethod(this, 'initialize');
        
        // postParams = 
        this._post_params = Sys.Serialization.JavaScriptSerializer.deserialize(this.get_element().control._post_params);
        
        //this.get_element().control._flashUrl,
        this._swfConfig = {
            flash_url: this.get_flashUrl(),
            upload_url: this.get_uploadUrl(),
            post_params: this.get_post_params(),
            file_size_limit: this.get_file_size_limit(),
            file_types: this.get_file_types(),
            file_types_description: this.get_file_types_description(),
            file_upload_limit: this.get_file_upload_limit(),
            file_queue_limit: this.get_file_queue_limit(),
            begin_upload_on_queue: this.get_begin_upload_on_queue(),
            
            debug: false,
            
            custom_settings: {
                progressTarget: this.get_progressTarget(),
                cancelButtonId: this.get_cancelButtonId(),
                scriptControlId: this.get_element().id,
				queue_cancelled_flag : false
            },
            
            file_queued_handler: this.FileQueuedHandler,
            file_queue_error_handler: this.FileQueueErrorHandler,
            file_dialog_complete_handler: this.FileDialogCompleteHandler,
            upload_start_handler: this.UploadStartHandler,
            upload_progress_handler: this.UploadProgressHandler,
            upload_error_handler: this.UploadErrorHandler,
            upload_success_handler: this.UploadSuccessHandler,
            upload_complete_handler: this.UploadCompleteHandler,
			
			// button settings
			button_placeholder_id : this.get_button_placeholder_id(),
			button_text : this.get_button_text(),
			button_width : this.get_button_width(),
			button_height: this.get_button_height(),
			
			
			
			swfupload_pre_load_handler : this._swfuploadPreLoad,
			swfupload_load_failed_handler : this._swfuploadLoadFailed,
			
			minimum_flash_version : "9.0.28",
			
		    file_post_name : this.get_file_post_name(),
		    use_query_string : this.get_use_query_string(),
		    requeue_on_error : this.get_requeue_on_error(),
		    http_success : this.get_http_success(),
		    prevent_swf_caching : this.get_prevent_swf_caching(),
		    button_image_url : this.get_button_image_url(),
		    button_text_style : this.get_button_text_style(),
		    button_text_left_padding : this.get_button_text_left_padding(),
		    button_text_top_padding : this.get_button_text_top_padding(),
		    button_action : this.get_button_action(),
		    button_disabled : this.get_button_disabled(),
		    button_cursor : this.get_button_cursor(),
		    button_window_mode : this.get_button_window_mode()
			
        
        };
       
/*
        this._swfConfig.button_placeholder_id = 'spanButtonPlaceholder';
        this._swfConfig.button_text = 'Upload...';
        this._swfConfig.button_width = 61;
        this._swfConfig.button_height = 22;
        this._swfConfig.swfupload_pre_load_handler = this._swfuploadPreLoad;
        this._swfConfig.swfupload_load_failed_handler = this._swfuploadLoadFailed;
        this._swfConfig.minimum_flash_version = "9.0.28";
*/
        
        this._swfUploader = new SWFUpload(this._swfConfig);
        
        
    },
    
    dispose: function(){
        $clearHandlers(this.get_element());
        this._swfUploader = null;
        SenseNet.Portal.UI.Controls.Upload.callBaseMethod(this, 'dispose');
    },
    
    // Properties /////////////////////////////////////////////////
	
	get_uploaderInstance: function() {
		return this._swfUploader;
	},
	get_uploaderConfig: function() {
		return this._swfConfig;
	},
	
    get_flashUrl: function(){
        return this._flashUrl;
    },
    set_flashUrl: function(value){
        if (this._flashUrl !== value) {
            this._flashUrl = value;
            this.raisePropertyChanged('flashUrl');
        }
    },
    
    get_progressTarget: function(){
        return this._progressTarget;
    },
    set_progressTarget: function(value){
        if (this._progressTarget !== value) {
            this._progressTarget = value;
            this.raisePropertyChanged('progressTarget');
        }
    },
    
    get_cancelButtonId: function(){
        return this._cancelButtonId;
    },
    set_cancelButtonId: function(value){
        if (this._cancelButtonId !== value) {
            this._cancelButtonId = value;
            this.raisePropertyChanged('cancelButtonId');
        }
    },
    
    get_uploadUrl: function(){
        return this._uploadUrl;
    },
    set_uploadUrl: function(value){
        if (this._uploadUrl !== value) {
            this._uploadUrl = value;
            this.raisePropertyChanged('uploadUrl');
        }
    },
    
    get_post_params: function(){
        return this._post_params;
    },
    set_post_params: function(value){
        if (this._post_params !== value) {
            this._post_params = value;
            //this._swfConfig.post_params = value;
            this.raisePropertyChanged('post_params');
        }
    },
    
    get_file_size_limit: function(){
        return this._file_size_limit;
    },
    set_file_size_limit: function(value){
        if (this._file_size_limit !== value) {
            this._file_size_limit = value;
            this.raisePropertyChanged('file_size_limit');
        }
    },
    
    get_file_types: function(){
        return this._file_types;
    },
    
    set_file_types: function(value){
        if (this._file_types !== value) {
            this._file_types = value;
            this.raisePropertyChanged('file_types');
        }
    },
    
    get_file_types_description: function(){
        return this._file_types_description;
    },
    
    set_file_types_description: function(value){
        if (this._file_types_description !== value) {
            this._file_types_description = value;
            this.raisePropertyChanged('file_types');
        }
    },
    
    get_file_upload_limit: function(){
        return this._file_upload_limit;
    },
    
    set_file_upload_limit: function(value){
        if (this._file_upload_limit !== value) {
            this._file_upload_limit = value;
            this.raisePropertyChanged('file_upload_limit');
        }
    },
    
    get_file_queue_limit: function(){
        return this._file_queue_limit;
    },
    
    set_file_queue_limit: function(value){
        if (this._file_queue_limit !== value) {
            this._file_queue_limit = value;
            this.raisePropertyChanged('file_queue_limit');
        }
    },
    
    get_begin_upload_on_queue: function(){
        return this._begin_upload_on_queue;
    },
    
    set_begin_upload_on_queue: function(value){
        if (this._begin_upload_on_queue !== value) {
            this._begin_upload_on_queue = value;
            this.raisePropertyChanged('begin_upload_on_queue');
        }
    },
    get_file_post_name: function(){
        return this._file_post_name;
    },
    
    set_file_post_name: function(value){
        if (this._file_post_name !== value) {
            this._file_post_name = value;
            this.raisePropertyChanged('file_post_name');
        }
    },
    get_use_query_string: function(){
        return this._use_query_string;
    },
    
    set_use_query_string: function(value){
        if (this._use_query_string !== value) {
            this._use_query_string = value;
            this.raisePropertyChanged('use_query_string');
        }
    },
    get_requeue_on_error: function(){
        return this._requeue_on_error;
    },
    
    set_requeue_on_error: function(value){
        if (this._requeue_on_error !== value) {
            this._requeue_on_error = value;
            this.raisePropertyChanged('requeue_on_error');
        }
    },
    get_http_success: function(){
        return this._http_success;
    },
    
    set_http_success: function(value){
        if (this._http_success !== value) {
            this._http_success = value;
            this.raisePropertyChanged('http_success');
        }
    },
    get_prevent_swf_caching: function(){
        return this._prevent_swf_caching;
    },
    
    set_prevent_swf_caching: function(value){
        if (this._prevent_swf_caching !== value) {
            this._prevent_swf_caching = value;
            this.raisePropertyChanged('prevent_swf_caching');
        }
    },
    get_button_placeholder_id: function(){
        return this._button_placeholder_id;
    },
    
    set_button_placeholder_id: function(value){
        if (this._button_placeholder_id !== value) {
            this._button_placeholder_id = value;
            this.raisePropertyChanged('button_placeholder_id');
        }
    },
    
    get_button_image_url: function(){
        return this._button_image_url;
    },
    
    set_button_image_url: function(value){
        if (this._button_image_url !== value) {
            this._button_image_url = value;
            this.raisePropertyChanged('button_image_url');
        }
    },
    get_button_width: function(){
        return this._button_width;
    },
    
    set_button_width: function(value){
        if (this._button_width !== value) {
            this._button_width = value;
            this.raisePropertyChanged('button_width');
        }
    },
    get_button_height: function(){
        return this._button_height;
    },
    
    set_button_height: function(value){
        if (this._button_height !== value) {
            this._button_height = value;
            this.raisePropertyChanged('button_height');
        }
    },
    get_button_text: function(){
        return this._button_text;
    },
    
    set_button_text: function(value){
        if (this._button_text !== value) {
            this._button_text = value;
            this.raisePropertyChanged('button_text');
        }
    },
    get_button_text_style: function(){
        return this._button_text_style;
    },
    
    set_button_text_style: function(value){
        if (this._button_text_style !== value) {
            this._button_text_style = value;
            this.raisePropertyChanged('button_text_style');
        }
    },
    get_button_text_left_padding: function(){
        return this._button_text_left_padding;
    },
    
    set_button_text_left_padding: function(value){
        if (this._button_text_left_padding !== value) {
            this._button_text_left_padding = value;
            this.raisePropertyChanged('button_text_left_padding');
        }
    },
	//button_text_top_padding
	get_button_text_top_padding: function(){
        return this._button_text_top_padding;
    },
    
    set_button_text_top_padding: function(value){
        if (this._button_text_top_padding !== value) {
            this._button_text_top_padding = value;
            this.raisePropertyChanged('button_text_top_padding');
        }
    },
    get_button_action: function(){
        return this._button_action;
    },
    
    set_button_action: function(value){
        if (this._button_action !== value) {
            this._button_action = value;
            this.raisePropertyChanged('button_action');
        }
    },
    get_button_disabled: function(){
        return this._button_disabled;
    },
    
    set_button_disabled: function(value){
        if (this._button_disabled !== value) {
            this._button_disabled = value;
            this.raisePropertyChanged('button_disabled');
        }
    },
    get_button_cursor: function(){
        return this._button_cursor;
    },
    
    set_button_cursor: function(value){
        if (this._button_cursor !== value) {
            this._button_cursor = value;
            this.raisePropertyChanged('button_cursor');
        }
    },
    get_button_window_mode: function(){
        return this._button_window_mode;
    },
    
    set_button_window_mode: function(value){
        if (this._button_window_mode !== value) {
            this._button_window_mode = value;
            this.raisePropertyChanged('button_window_mode');
        }
    },
    get_debug: function() {
        return this._debug;
    },

    set_debug: function(value) {
    if (this._debug !== value) {
        this._debug = value;
        this.raisePropertyChanged('debug');
        }
    },
       
    // events called by SWFUpload
    
    FileQueuedHandler: function(file){
    
        var component = $find(this.customSettings.scriptControlId);
        
        component.RenderFileProgressWrapperElements(file, this.customSettings.progressTarget);
        component.SetStatusMessage("Pending...");
        component.ToggleCancelButton(true, this);
    },
    
    FileQueueErrorHandler: function(file, errorCode, message){
        if (errorCode === SWFUpload.QUEUE_ERROR.QUEUE_LIMIT_EXCEEDED) {
            alert("You have attempted to queue too many files.\n" + (message === 0 ? "You have reached the upload limit." : "You may select " + (message > 1 ? "up to " + message + " files." : "one file.")));
            return;
        }
        
        var component = $find(this.customSettings.scriptControlId);
        component.RenderFileProgressWrapperElements(file, this.customSettings.progressTarget);
        component.SetError();
        component.ToggleCancelButton(false);
        
        switch (errorCode) {
            case SWFUpload.QUEUE_ERROR.FILE_EXCEEDS_SIZE_LIMIT:
                component.SetStatusMessage("File is too big.");
                //this.debug("Error Code: File too big, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.QUEUE_ERROR.ZERO_BYTE_FILE:
                component.SetStatusMessage("Cannot upload Zero Byte files.");
                //this.debug("Error Code: Zero byte file, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.QUEUE_ERROR.INVALID_FILETYPE:
                component.SetStatusMessage("Invalid File Type.");
                //this.debug("Error Code: Invalid File Type, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            default:
                if (file !== null) {
                    component.SetStatusMessage("Unhandled Error");
                }
                //this.debug("Error Code: " + errorCode + ", File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
        }
    },
    
    FileDialogCompleteHandler: function(numFilesSelected, numFilesQueued){
        if (numFilesSelected > 0) {
            $get(this.customSettings.cancelButtonId).disabled = false;
        }
        
        //TODO: automatic upload!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        
        this.startUpload();
    },
    
    UploadStartHandler: function(file){
        try {
        
            // show Uploading status message
            var component = $find(this.customSettings.scriptControlId);
            component.RenderFileProgressWrapperElements(file, this.customSettings.progressTarget);
            component.SetStatusMessage("Uploading...");
            component.ToggleCancelButton(true, this);
        } 
        catch (ex) {
        }
        
        return true; // return true to indicate that the upload should start
    },
    
    UploadProgressHandler: function(file, bytesLoaded, bytesTotal){
        //
        try {
            var percent = Math.ceil((bytesLoaded / bytesTotal) * 100);
            var component = $find(this.customSettings.scriptControlId);
            
            component.RenderFileProgressWrapperElements(file, this.customSettings.progressTarget);
            component.SetProgressBar(percent);
            component.SetStatusMessage("Uploading...");
        } 
        catch (ex) {
        
        }
    },
    
    UploadSuccessHandler: function(file, serverData){
        var component = $find(this.customSettings.scriptControlId);
        component.RenderFileProgressWrapperElements(file, this.customSettings.progressTarget);
        component.SetCompleteMessage();
        component.SetStatusMessage("Complete.");
        component.ToggleCancelButton(false);
        component.UploadSuccessHandlerCallback();
    },
    
    addUploadSuccessHandler: function(handler){
        this.get_events().addHandler("uploadSuccess", handler);
    },
    
    removeUploadSuccessHandler: function(handler){
        this.get_events().removeHandler('uploadSuccess', handler);
    },
    
    UploadSuccessHandlerCallback: function(e){
        var h = this.get_events().getHandler('uploadSuccess');
        if (h) 
            h(this, Sys.EventArgs.Empty);
    },
    
    UploadErrorHandler: function(file, errorCode, message){
        var component = $find(this.customSettings.scriptControlId);
        component.RenderFileProgressWrapperElements(file, this.customSettings.progressTarget);
        component.SetError();
        component.ToggleCancelButton(false);
        
        switch (errorCode) {
            case SWFUpload.UPLOAD_ERROR.HTTP_ERROR:
                component.SetStatusMessage("Upload Error: " + message);
                //this.debug("Error Code: HTTP Error, File name: " + file.name + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.UPLOAD_FAILED:
                component.SetStatusMessage("Upload Failed.");
                //this.debug("Error Code: Upload Failed, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.IO_ERROR:
                component.SetStatusMessage("Server (IO) Error");
                //this.debug("Error Code: IO Error, File name: " + file.name + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.SECURITY_ERROR:
                component.SetStatusMessage("Security Error");
                //this.debug("Error Code: Security Error, File name: " + file.name + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.UPLOAD_LIMIT_EXCEEDED:
                component.SetStatusMessage("Upload limit exceeded.");
                //this.debug("Error Code: Upload Limit Exceeded, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.FILE_VALIDATION_FAILED:
                component.SetStatusMessage("Failed Validation.  Upload skipped.");
                //this.debug("Error Code: File Validation Failed, File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
            case SWFUpload.UPLOAD_ERROR.FILE_CANCELLED:
                // If there aren't any files left (they were all cancelled) disable the cancel button
                if (this.getStats().files_queued === 0) {
                    $get(this.customSettings.cancelButtonId).disabled = true;
                }
                component.SetStatusMessage("Cancelled");
                component._setCancelled();
                break;
            case SWFUpload.UPLOAD_ERROR.UPLOAD_STOPPED:
                component.SetStatusMessage("Stopped");
                break;
            default:
                component.SetStatusMessage("Unhandled Error: " + errorCode);
                //this.debug("Error Code: " + errorCode + ", File name: " + file.name + ", File size: " + file.size + ", Message: " + message);
                break;
        }
    },
    
    UploadCompleteHandler: function(file){
        if (this.getStats().files_queued > 0) { // go on until there is file in queue :)
            this.startUpload();
        }
        else {
            $get(this.customSettings.cancelButtonId).disabled = true;
            var component = $find(this.customSettings.scriptControlId);
            component.UploadCompleteHandlerCallback();
            
        }
    },
    
    addUploadCompleteHandler: function(handler){
        this.get_events().addHandler('uploadComplete', handler);
    },
    
    removeUploadCompleteHandler: function(handler){
        this.get_events().removeHandler('uploadComplete', handler);
    },
    
    UploadCompleteHandlerCallback: function(){
        var h = this.get_events().getHandler('uploadComplete');
        if (h) 
            h(this, Sys.EventArgs.Empty);
    },
    
    // progressbar methods
    RenderFileProgressWrapperElements: function(file, targetId){
    
    
        this.fileProgressID = file.id;
        this._fileProgressWrapper = $get(this.fileProgressID);
        
        if (!this._fileProgressWrapper) {
            this._fileProgressWrapper = document.createElement("div");
            this._fileProgressWrapper.className = "sn-progress-wrapper";
            this._fileProgressWrapper.id = this.fileProgressID;
            
            this._fileProgressElement = document.createElement("div");
            this._fileProgressElement.className = "sn-progress-container";

            var progressCancel = document.createElement("a");
            progressCancel.className = "sn-progress-cancel";
            progressCancel.href = "#";
            progressCancel.style.visibility = "hidden";
            progressCancel.appendChild(document.createTextNode(" "));
            
            var progressText = document.createElement("div");
            progressText.className = "sn-progress-name";
            progressText.appendChild(document.createTextNode(file.name));
            
            var progressBar = document.createElement("div");
            progressBar.className = "sn-progressbar-inprogress";
            
            var progressStatus = document.createElement("div");
            progressStatus.className = "sn-progressbar-status";
            progressStatus.innerHTML = "&nbsp;";

            this._fileProgressElement.appendChild(progressCancel);
            this._fileProgressElement.appendChild(progressText);
            this._fileProgressElement.appendChild(progressStatus);
            this._fileProgressElement.appendChild(progressBar);
            
            this._fileProgressWrapper.appendChild(this._fileProgressElement);
            
            $get(targetId).appendChild(this._fileProgressWrapper);
        }
        else {
            this._fileProgressElement = this._fileProgressWrapper.firstChild;
        }
        
        //this._fileProgressHeight = $get(this.customSettings.progressTarget).offsetHeight;
    
    },
    
    SetProgressBar: function(percentage){
        this._fileProgressElement.className = 'sn-progress-container sn-progress-ok'; // progress bar 1px height
        this._fileProgressElement.childNodes[3].className = 'sn-progressbar-inprogress'; // in progress classname
        this._fileProgressElement.childNodes[3].style.width = percentage + "%"; // dynamically sets the progressbar width
    },
    
    SetCompleteMessage: function(){
    
        //TODO: do something strange when upload is completed

    this._fileProgressElement.className = 'sn-progress-container sn-progress-info';
        this._fileProgressElement.childNodes[3].className = 'sn-progressbar-complete';
        this._fileProgressElement.childNodes[3].style.width = "";
    },
    
    SetError: function(){
    
        //TODO: do something strange when an exception has been thrown

    this._fileProgressElement.className = 'sn-progress-container sn-progress-error';
        this._fileProgressElement.childNodes[3].className = 'sn-progressbar-error';
        this._fileProgressElement.childNodes[3].style.width = '';
    },
    
    _setCancelled: function(){
    
        //TODO: do something strange when user has just broken uploading
        
        this._fileProgressElement.className = 'sn-progress-container';
        this._fileProgressElement.childNodes[3].className = 'sn-progressbar-error';
        this._fileProgressElement.childNodes[3].style.width = '';
    },
    
    SetStatusMessage: function(status){
    
        // TODO: set status message
        
        this._fileProgressElement.childNodes[2].innerHTML = status;
    },
    
    ToggleCancelButton: function(show, swfuploadInstance){
    
        // dynamically shows and hides cancel button
        
        this._fileProgressElement.childNodes[0].style.visibility = show ? "visible" : "hidden";
        if (swfuploadInstance !== null) {
            var fileID = this.fileProgressID;
            this._fileProgressElement.childNodes[0].onclick = function(){
                swfuploadInstance.cancelUpload(fileID);
                return false;
            };
        }
    }
}

var FileUploadUtility = {
    selectFiles: function(uploaderInstance) {
        $find(uploaderInstance).get_uploaderInstance().selectFiles();
    },

    selectFile: function(uploaderInstance) {
        $find(uploaderInstance).get_uploaderInstance().selectFile();
    },

    stopUpload: function(uploaderInstance) {
        var uploader = $find(uploaderInstance).get_uploaderInstance();
        if (uploader !== null) {
            uploader.customSettings.queue_cancelled_flag = true;
            uploader.stopUpload();
            var stats = uploader.getStats();
            while (stats.files_queued > 0) {
                uploader.cancelUpload();
                stats = uploader.getStats();
            }
        }

    },

    addContentType: function(uploaderInstance, selectedItem) {
        var component = $find(uploaderInstance);
        if (component) {

            var instance = component._swfUploader;
            instance.addPostParam('ContentType', selectedItem);
        }
    }

}




SenseNet.Portal.UI.Controls.Upload.registerClass('SenseNet.Portal.UI.Controls.Upload', Sys.UI.Control);
if (typeof(Sys) !== 'undefined') 
    Sys.Application.notifyScriptLoaded();