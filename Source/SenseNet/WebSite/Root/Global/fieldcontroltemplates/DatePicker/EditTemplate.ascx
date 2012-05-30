<%@ Language="C#" %>
<asp:PlaceHolder ID="InnerDateHolder" runat="server">
    Date <asp:TextBox ID="InnerControl" runat="server" CssClass="sn-ctrl sn-ctrl-text sn-ctrl-date" style="width:100px;"></asp:TextBox>
</asp:PlaceHolder>
<asp:PlaceHolder ID="InnerTimeHolder" runat="server">
    Time <asp:TextBox ID="InnerTimeTextBox" runat="server" CssClass="sn-ctrl sn-ctrl-text sn-ctrl-time" style="width:100px;"></asp:TextBox>
</asp:PlaceHolder>
<asp:Label ID="DateFormatLabel" runat="server" CssClass="sn-iu-desc" /><br /><asp:Label ID="TimeFormatLabel" runat="server" CssClass="sn-iu-desc" />
