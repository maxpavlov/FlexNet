<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<div class="sn-pt-body-border ui-widget-content" >
     <div class="sn-pt-body" >
         <asp:Panel ID="MovePanel" runat="server">
            You are going to move <strong><asp:Label ID="ContentName" runat="server" /></strong>. <br/>
            Please choose a folder or content list to move to: <br />
            <asp:TextBox runat="server" ID="MoveTargetTextBox" Width="300px" />
            <asp:Button runat="server" ID="OpenPickerButton" Text="Select..." CssClass="sn-submit" />
         </asp:Panel>
         <asp:Panel ID="MessagePanel" runat="server" Visible="false">
            You successfully moved the specified content.
         </asp:Panel>
         <asp:PlaceHolder runat="server" ID="ErrorPanel" Visible="false">
             <div class="sn-error-msg">
                <asp:Label runat="server" ID="ErrorLabel" />
             </div>
         </asp:PlaceHolder>
     </div> 
</div>

<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <asp:Button ID="MoveCurrentButton" runat="server" Text="Move" CssClass="sn-submit" />
        <sn:BackButton Text="Done" ID="DoneButton" runat="server" CssClass="sn-submit" Visible="False"/>
        <sn:BackButton Text="Cancel" ID="CancelButton" runat="server" CssClass="sn-submit" />
    </div>
</div>