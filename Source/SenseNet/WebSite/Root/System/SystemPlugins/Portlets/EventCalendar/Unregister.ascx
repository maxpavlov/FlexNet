<%@ Control Language="C#" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<sn:MessageControl CssClass="snConfirmDialog" ID="MessageControl" runat="server" Buttons="YesNo">
     <footertemplate>
        <div class="sn-pt-footer"></div>    
    
        <div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
            <div class="sn-pt-body">

                <asp:Label CssClass="sn-confirmquestion" ID="RusLabel" runat="server" Text="<span class='sn-icon-big sn-icon-button snIconBig_warning'></span> Are you sure you want to unregister from the event?" />
 
                <asp:Button ID="OkBtn" runat="server" Text="Ok" CommandName="Ok" CssClass="sn-submit" Visible="false" /> 
                <asp:Button ID="YesBtn" runat="server" Text="Yes" CommandName="Yes" CssClass="sn-submit" Visible="true" /> 
                <asp:Button ID="NoBtn" runat="server" Text="No" CommandName="No" CssClass="sn-submit" Visible="true" /> 
                <asp:Button ID="CancelBtn" runat="server" Text="Cancel" CommandName="Cancel" CssClass="sn-submit" Visible="false" /> 
            </div>
        </div>
        <div class="sn-pt-footer"></div>    
    </footertemplate>
</sn:MessageControl>
