<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:DropDownList ID="ddFieldName" runat="server" Width="180px" /> 

<asp:DropDownList ID="ddOrder" runat="server" Width="90px">
 <asp:ListItem Text="Ascending" Value="ASC" />
 <asp:ListItem Text="Descending" Value="DESC" />
</asp:DropDownList>