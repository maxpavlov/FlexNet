<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

 <div class="sn-pt-body-border ui-widget-content">
     <div class="sn-pt-body">
         <p class="sn-lead sn-dialog-lead">
             You are about to approve or reject <strong><asp:Label ID="ContentName" runat="server" /></strong>
         </p>
         <asp:PlaceHolder runat="server" ID="ErrorPanel">
             <div class="sn-error-msg">
                <asp:Label runat="server" ID="ErrorLabel" />
             </div>
         </asp:PlaceHolder>
     </div>
</div>   
        
<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <asp:Button ID="Approve" runat="server" Text="Approve" CommandName="Approve" CssClass="sn-submit" />
        <asp:Button ID="Reject" runat="server" Text="Reject" CommandName="Reject" CssClass="sn-submit" />
        <sn:BackButton Text="Cancel" ID="BackButton1" runat="server" CssClass="sn-submit" />
    </div>
</div>
