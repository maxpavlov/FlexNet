<%@  Language="C#" %>
<%# DataBinder.Eval(Container, "Data") %>   
<asp:PlaceHolder ID="plcInheritedInfo" runat="server" Visible="false">
    <br /> Value: <asp:Label ID="InheritedValueLabel" runat="server" />
</asp:PlaceHolder>
