<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

 <div class="sn-pt-body-border ui-widget-content">
     <div class="sn-pt-body">
         <p class="snContentLead snDialogLead">
             You are about to approve or reject subscription:
         </p>
         <p>
         Subscriber: <%Eval("CreatedBy") %></br>
         Subscription date: <%Eval("CreationDate") %>
         </p>
         <asp:PlaceHolder runat="server" ID="ErrorPanel">
             <div style="background-color:Red;font-weight:bold;color:White">
                <asp:Label runat="server" ID="ErrorLabel" />
             </div>
         </asp:PlaceHolder>
     </div>
</div>   
        
<div class="sn-pt-body-border ui-widget-content snDialogButtons">
    <div class="sn-pt-body">
        <asp:Button ID="Approve" runat="server" Text="Approve" CommandName="Approve" CssClass="sn-submit" />
        <asp:Button ID="Reject" runat="server" Text="Reject" CommandName="Reject" CssClass="sn-submit" />
        <sn:BackButton Text="Cancel" ID="BackButton1" runat="server" CssClass="sn-submit" />
    </div>
</div>
