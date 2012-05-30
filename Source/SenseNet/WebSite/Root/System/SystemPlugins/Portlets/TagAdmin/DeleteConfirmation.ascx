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
                        <asp:Label ID="DeleteOneLabel" runat="server" Text="You are about to delete <strong>{0}</strong> tag from <strong>{1}</strong>" Visible="false" /> 
                    </p>                          
             
                    <asp:CheckBox CssClass="sn-warning-msg" runat="server" ID="PermanentDeleteCheckBox" Text="<span class='sn-icon-small sn-icon-button snIconSmall_error'></span>By checking this box, you may choose to permanently delete contents, instead of moving them to the trash.<br/><br/>" Visible="false" />
                    <asp:Label CssClass="sn-warning-msg" ID="PermanentDeleteLabel" runat="server" Text="<span class='sn-icon-small sn-icon-button snIconSmall_error'></span>Tag will be permanently deleted!<br/><br/>" Visible="false" />
                
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
