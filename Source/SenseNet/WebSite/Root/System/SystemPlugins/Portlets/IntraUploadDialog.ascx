<%@ Control Language="C#" AutoEventWireup="true" %>
<%--<script type="text/jscript">
    function ToggleMultipleFilesDisplay(hrefEl, divClientId) {
        var el = document.getElementById(divClientId);
        if (el.style.display == 'none') {
            hrefEl.innerHTML = 'Close multiple file uploader...';
            el.style.display = '';
        } else {
            hrefEl.innerHTML = 'Show multiple file uploader...';
            el.style.display = 'none';
        }
    }
</script>--%>

<%--<div style="height: 100px">
    <div class="sn-inputunit ui-helper-clearfix">
        <div class="sn-iu-label">
            <label for="simpleUpload" class="sn-iu-title">
                Upload document</label><br />
            <label for="simpleUpload" class="sn-iu-desc">
                Browse to the document you insert to upload</label>
        </div>
        <div class="sn-iu-control">
            <div style="padding-left: 10px">
                <p>
                    Name:</p>
                <div>
                    <sn:simpleupload id="simpleUpload" submitbuttonid="submitButtonId" width="300" runat="server"></sn:simpleupload>
                </div>
                            <br />
                <a id="showMultipleUploader" href="#" onclick="ToggleMultipleFilesDisplay(this,'sn-upload-multiple');return false;">
                    Show multiple file uploader...</a>

            </div>
        </div>
    </div>
</div>--%>

<div id="sn-upload-multiple">             
    <sn:listviewupload id="FileUploader" runat="server" cssclass="sn-dialog-upload" flashurl="/Root/Global/scripts/swfupload/swfupload.swf"
        uploadurl="/UploadProxy.ashx" begin_upload_on_queue="true" file_queue_limit="0"
        file_size_limit="8192" file_types="*.*" file_types_description="All files" file_upload_limit="0"
        allowothercontenttype="true" designmode="dialog" button_placeholder_id="spanButtonPlaceholder"
        minimum_flash_version="9.0.28" button_window_mode="transparent" button_cursor="-2"
        button_width="150" button_height="22" />
</div>

<div class="sn-panel sn-buttons">
<%--    <asp:Button ID="submitButtonId" runat="server" Text="OK" />--%>
    <sn:BackButton CssClass="sn-submit" runat="server" text="Done" id="BackButton" />
</div>
