<%@ Control Language="C#" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<sn:MessageControl CssClass="snConfirmDialog" ID="MessageControl" runat="server"
    Buttons="YesNo">
    <headertemplate>
        <div class="sn-pt-body-border ui-widget-content sn-dialog-confirmation">
            <div class="sn-pt-body">
            
                <asp:Panel CssClass="sn-dialog-icon sn-dialog-trash" runat="server" id="DialogIcon" />

    </headertemplate>
    <controltemplate>
                <asp:PlaceHolder ID="DialogHeader" runat="server">

                    <p class="sn-lead sn-dialog-lead">
                        <asp:Label ID="DeleteOneLabel" runat="server" Text="You are about to delete <strong>{0}</strong> from <strong>{1}</strong>" Visible="false" /> 
                        <asp:Label ID="DeleteFolderLabel" runat="server" Text="You are about to delete <strong>{0}</strong> and all of its contents and subfolders from <strong>{1}</strong>" Visible="false" />        

                    </p>

                    <asp:PlaceHolder ID="DialogProperties" runat="server">
                        <ul class="sn-dialog-properties">
                            <li>
                                <asp:Label runat="server" ID="BinEnabledLabel" Text="Trash Bin is enabled and configured, so your deleted contents will be moved to the trash." />
                                <asp:Label runat="server" ID="BinDisabledGlobalLabel" Text="Trash Bin has been disabled globally." Visible="false" />
                                <asp:Label runat="server" ID="BinNotConfiguredLabel" Text="Trash Bin is not configured for the container from which you are about to delete." Visible="false" />
                                <asp:Label runat="server" ID="PurgeFromTrashLabel" Text="Permanent delete" Visible="false" />
                            </li>
                        </ul>                        
                    </asp:PlaceHolder>
                          
             
                    <asp:CheckBox style="display:block; margin-top:10px;" runat="server" CssClass="sn-checkbox" ID="PermanentDeleteCheckBox" Text="By checking this box, you choose to permanently delete content, instead of moving them to the trash." Visible="false" />
                    <asp:Label style="display:block; margin-top:10px;" ID="TooMuchContentLabel" runat="server" Text="<span class='sn-icon-small sn-icon-button snIconSmall_error'></span>The number of content to be deleted is <strong>too much</strong> to be moved to the trash at once. By clicking Yes, you choose to delete permanently or you can go back and try to delete a smaller number of content." Visible="false" />
                    <asp:Label style="display:block; margin-top:10px;" CssClass="sn-warning-msg" ID="PermanentDeleteLabel" runat="server" Text="<span class='sn-icon-small sn-icon-button snIconSmall_error'></span>Content will be permanently deleted!" Visible="false" />
                
                </asp:PlaceHolder>
    </controltemplate>
    <confirmationtemplate>
                <asp:Panel CssClass="sn-message" ID="DialogMessagePanel" runat="server" Visible="false">
                    <asp:Label CssClass="sn-icon-big sn-icon-left snIconBig_warning" ID="DialogMessageIcon" runat="server" />
                    <asp:Label CssClass="sn-msg-title" ID="DialogMessage" runat="server" /><br />
                    <asp:Label CssClass="sn-msg-description" ID="DialogDescription" runat="server" />
                    <asp:Label ID="ErrorMessage" runat="server" Visible="false" CssClass="sn-error" />
                </asp:Panel>
    </confirmationtemplate>
    <footertemplate>

            </div>
        </div>    
        <div class="sn-pt-footer"></div>    
    
        <div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
            <div class="sn-pt-body">

                <asp:Label CssClass="sn-confirmquestion" ID="RusLabel" runat="server" Text="<span class='sn-icon-big sn-icon-button snIconBig_warning'></span> Are you sure?" />

                <asp:Button ID="OkBtn" runat="server" Text="Ok" CommandName="Ok" CssClass="sn-submit" Visible="false" /> 
                <asp:Button ID="YesBtn" runat="server" Text="Yes" CommandName="Yes" CssClass="sn-submit" Visible="true" /> 
                <asp:Button ID="NoBtn" runat="server" Text="No" CommandName="No" CssClass="sn-submit" Visible="true" /> 
                <asp:Button ID="CancelBtn" runat="server" Text="Cancel" CommandName="Cancel" CssClass="sn-submit" Visible="false" /> 
            </div>
        </div>
        <div class="sn-pt-footer"></div>    
    </footertemplate>
</sn:MessageControl>
