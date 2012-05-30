<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<sn:MessageControl ID="RestoreMessage" runat="server" >
    <HeaderTemplate>
        <div class="sn-pt-body-border ui-widget-content">
            <div class="sn-pt-body">
                <div class="sn-dialog-icon sn-dialog-restore"></div>
                <p class="sn-lead sn-dialog-lead">
                    Error restoring <strong><asp:Label ID="LabelContent" runat="server" /></strong>
                </p>
                <div class="sn-message">
                    <span class="sn-icon-big sn-icon-left snIconBig_warning"></span>
                    <asp:Label CssClass="sn-msg-title" ID="LabelMessage" runat="server" /><br />
                    <asp:Label CssClass="sn-msg-description" ID="LabelDesc" runat="server" />
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
                <asp:Button ID="NewNameBtn" runat="server" Text="Restore with new name" CommandName="RestoreWithNewName" CssClass="sn-submit" /> 
                <asp:Button ID="OkBtn" runat="server" Text="Ok" CommandName="Ok" CssClass="sn-submit" /> 
            </div>
        </div>
        <div class="sn-pt-footer"></div>    
    </FooterTemplate>
</sn:MessageControl>