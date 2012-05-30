<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<div class="sn-pt-body-border ui-widget-content" >
     <div class="sn-pt-body" >
         <asp:Panel ID="MovePanel" runat="server">
            You are going to move these contents to <strong><asp:Label ID="ContentName" runat="server" /></strong>.
            Are you sure?
         </asp:Panel>
         <asp:Panel ID="MessagePanel" runat="server" Visible="false">
            You successfully moved the specified contents.
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
        <asp:Button ID="MoveToButton" runat="server" Text="Move" CssClass="sn-submit" />
        <sn:BackButton Text="Done" ID="DoneButton" runat="server" CssClass="sn-submit" Visible="False"/>
        <sn:BackButton Text="Cancel" ID="CancelButton" runat="server" CssClass="sn-submit" />
    </div>
</div>