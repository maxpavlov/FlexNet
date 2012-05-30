<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl"  %>

<div class="sn-param-search">
    <div class="sn-content-subtitle">DisplayName: </div>
    <asp:TextBox ID="defaultInput" runat = "server" CssClass="sn-ctrl-text sn-ctrl-medium ui-widget-content ui-corner-all"></asp:TextBox>
    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="sn-button sn-submit ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only" />
</div>
<asp:Panel ID="EmptyQueryErrorPanel" runat="server" Visible="false">
Empty queries are not allowed.
</asp:Panel>
