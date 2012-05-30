<%@ Control Language="C#" AutoEventWireup="true" %>
<div class="sn-content">
    <h1 class="sn-content-title">Upload</h1>
    <div class="sn-lead">
        Description of the action, that can be broken into two or more lines, if the text is long.
    </div>
    <sn:Toolbar ID="Toolbar1" runat="server">
        <sn:Upload ID="FileUploader" 
            runat="server" 
            CssClass="sn-dialog-upload sn-toolbar-inner"
            FlashUrl="/Root/Global/scripts/swfupload/swfupload.swf" 
            UploadUrl="/UploadProxy.ashx" 
            Begin_upload_on_queue="true" 
            File_queue_limit="0" 
            File_size_limit="8192" 
            File_types="*.*" 
            File_types_description="All files" 
            File_upload_limit="0" 
            AllowOtherContentType="false"
            DesignMode="dialog"
            button_placeholder_id="spanButtonPlaceholder"
            minimum_flash_version="9.0.28"
            button_window_mode="transparent"
            button_cursor="-2"
            button_width="150"
            button_height="22"
        />        
    </sn:Toolbar>
    <div class="sn-panel sn-buttons">
        <input type="submit" class="sn-submit" value="Done" />
    </div>
</div>