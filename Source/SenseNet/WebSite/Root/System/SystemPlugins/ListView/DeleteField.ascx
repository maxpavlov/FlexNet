<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:Panel ID="pnlDelete" runat="server">
<div class="sn-dialog-confirmation">
<p class="sn-lead sn-dialog-lead">
    You are about to delete <asp:Label ID="lblFieldName" runat="server" />
</p>
</div>
<div class="sn-dialog-buttons">
   <div class="sn-pt-body">
   <asp:Label CssClass="sn-confirmquestion" ID="RusLabel" runat="server" Text="<span class='sn-icon-big sn-icon-button snIconBig_warning'></span> Are you sure?" />

    <asp:Button ID="btnDelete" runat="server" CssClass="sn-submit" Text="Delete" /> 
    <asp:Button ID="btnCancel" runat="server" CssClass="sn-submit" Text="Cancel" />
   </div>
</div>

</asp:Panel>

