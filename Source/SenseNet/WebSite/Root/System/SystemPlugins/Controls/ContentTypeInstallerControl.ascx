<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.Controls.ContentTypeInstallerControl" %>

<asp:Panel ID="pnlInstall" runat="server" Visible="true">
    <div class="sn-pt-body-border ui-widget-content" >
        <div class="sn-pt-body" >
            You are going to install the CTD definition above. <br/>
        </div> 
    </div>

    <div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
        <div class="sn-pt-body">
            <asp:Button ID="InstallerButton" runat="server" Text="Install" CssClass="sn-submit" OnClick="InstallerButton_Click" />
            <sn:BackButton Text="Cancel" ID="CancelButton" runat="server" CssClass="sn-submit" />
        </div>
    </div>
</asp:Panel>
<asp:Panel ID="pnlSuccess" runat="server" Visible="false">
    <div class="sn-pt-body-border ui-widget-content" >
        <div class="sn-pt-body" >
            The CTD has been successfully installed. <br/>
        </div> 
    </div>

    <div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
        <div class="sn-pt-body">
            <sn:BackButton Text="Done" ID="CancelButton1" runat="server" CssClass="sn-submit" />
        </div>
    </div>
</asp:Panel>
