<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<div class="sn-pt-body-border ui-widget-content" >
     <div class="sn-pt-body" >
         <asp:Panel ID="CopyPanel" runat="server">
            You are going to copy <strong><asp:Label ID="ContentName" runat="server" /></strong>. <br/>
            Please choose a folder or content list to copy to: <br />
            <asp:TextBox runat="server" ID="CopyTargetTextBox" Width="300px" />
            <asp:Button runat="server" ID="OpenPickerButton" Text="Select..." CssClass="sn-submit"/>
                     
         </asp:Panel>
         <asp:Panel ID="MessagePanel" runat="server" Visible="false">
            You successfully copied the specified content.
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
        <asp:Button ID="CopyCurrentButton" runat="server" Text="Copy" CssClass="sn-submit" />
        <sn:BackButton Text="Done" ID="DoneButton" runat="server" CssClass="sn-submit" Visible="False"/>
        <sn:BackButton Text="Cancel" ID="CancelButton" runat="server" CssClass="sn-submit" />
    </div>
</div>