<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:Panel CssClass="sn-quicksearch" runat="server" ID="quickPanel" DefaultButton="QuickSearchButton">
    <span class="sn-quicksearch-text"><asp:TextBox ID="SearchBox" runat="server" /></span>
    <asp:Button ID="QuickSearchButton" Text="Search" runat="server" CssClass="sn-quicksearch-button" UseSubmitBehavior="true" />
</asp:Panel>