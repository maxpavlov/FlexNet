<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<sn:MessageControl ID="RestoreMessage" runat="server" >
    <HeaderTemplate>
        <div class="sn-pt-body-border ui-widget-content">
            <div class="sn-pt-body">
                <div class="sn-dialog-icon sn-dialog-restore"></div>
                <p class="sn-lead sn-dialog-lead">
                    You are about to restore <strong><asp:Label ID="LabelContent" runat="server" /></strong> from the Trash to the following destination
                </p>
                <div>
                    <asp:TextBox ID="Destination" CssClass="sn-ctrl sn-ctrl-text" style="width:500px" runat="server" /> 
                    <asp:Button ID="DestinationPicker" runat="server" Text="..." /> 
                </div>

            </div>
        </div>    
        <div class="sn-pt-footer"></div>    
    </HeaderTemplate>
    <ControlTemplate>
    </ControlTemplate>    
    <FooterTemplate>
        <div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
            <div class="sn-pt-body">
                <asp:Button ID="OkBtn" runat="server" Text="Restore" CommandName="Restore" CssClass="sn-submit" /> 
                <asp:Button ID="CancelBtn" runat="server" Text="Cancel" CommandName="Cancel" CssClass="sn-submit" />
            </div>
        </div>
        <div class="sn-pt-footer"></div>    
    </FooterTemplate>
</sn:MessageControl>
