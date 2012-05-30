<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:Panel ID="pnlDelete" runat="server" HorizontalAlign="Center" Font-Bold="true">
    You are about to delete <asp:Label ID="lblFieldName" runat="server" />
    <br/>
    Are you sure?
    <br/>
    <asp:Button ID="btnDelete" runat="server" Text="Delete" /> 
    <asp:Button ID="btnCancel" runat="server" Text="Cancel" />
</asp:Panel>
