<%@  Language="C#" %>
<asp:Panel ID="Container" runat="server">
    <div>
        <asp:Button ID="UploadButton" runat="server" Text="Upload" onclientclick="SN.DialogUpload.openUploadDialog($(this));return false;" CssClass="sn-button sn-notdisabled" />

        <div class="sn-du-uploadloading" style="display:none;">Loading links...</div>
        <div class="sn-du-uploadlinks"></div>
    </div>
	
    <asp:TextBox ID="StartUploadDate" runat="server" class="sn-du-startuploaddate" style="display:none;" />
    <asp:TextBox ID="ClientIdControl" runat="server" class="sn-du-clientid" style="display:none;" />
    <asp:TextBox ID="UploadPath" runat="server" class="sn-du-uploadpath" style="display:none;" />
    <asp:TextBox ID="TargetFolder" runat="server" class="sn-du-targetfolder" style="display:none;" />

    <asp:Panel ID="UploadDialog" runat="server" style="padding:0; overflow:hidden;" >
    
        <asp:TextBox ID="DialogClientIdControl" runat="server" class="sn-du-dialogclientid" style="display:none;" />
        <iframe class="sn-du-uploadframe" style="padding:5px;" width="320" height="200" frameborder=0 src=""></iframe>

        <div class="sn-sharecontent-buttoncontainer" style="margin-top:0;">
            <div class="sn-sharecontent-buttondiv">
                <input type="button" class="sn-submit sn-button sn-notdisabled" value="Close" onclick="SN.DialogUpload.closeUploadDialog($(this));return false;" />
            </div>
        </div>

    </asp:Panel>

</asp:Panel>

