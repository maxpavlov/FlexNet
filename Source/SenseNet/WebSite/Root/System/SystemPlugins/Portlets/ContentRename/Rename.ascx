<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<div class="sn-pt-body-border ui-widget-content" >
     <div class="sn-pt-body" >
            You are going to rename <strong><asp:Label ID="ContentName" runat="server" /></strong>. <br/>
            Please choose a new name: 
            <br />
            <br />
            <asp:PlaceHolder runat="server" ID="ContentViewPlaceHolder"></asp:PlaceHolder>
                     
         <asp:PlaceHolder runat="server" ID="ErrorPanel" Visible="false">
             <div class="sn-error-msg">
                <asp:Label runat="server" ID="ErrorLabel" />
             </div>
         </asp:PlaceHolder>
     </div> 
</div>

<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <asp:Button ID="RenameButton" runat="server" Text="Rename" CssClass="sn-submit" />
        <sn:BackButton Text="Cancel" ID="CancelButton" runat="server" CssClass="sn-submit" />
    </div>
</div>