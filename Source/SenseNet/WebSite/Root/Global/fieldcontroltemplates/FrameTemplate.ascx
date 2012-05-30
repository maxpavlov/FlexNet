<%@  Language="C#" %>
<div class="sn-inputunit ui-helper-clearfix" runat="server" ID="InputUnitPanel">
    <div class="sn-iu-label">
        <asp:Label CssClass="sn-iu-title" ID="LabelForTitle" runat="server"></asp:Label>
        <asp:Label CssClass="sn-iu-required-mark" ID="ControlForRequired" runat="server" Text="*" Visible="false"/><br />
        <asp:Label CssClass="sn-iu-desc" ID="LabelForDesc" runat="server"></asp:Label>
    </div>
    <div class="sn-iu-control">
        <asp:PlaceHolder ID="ControlPlaceHolder" runat="server" />
        <asp:PlaceHolder Visible="false" ID="ErrorPlaceHolder" runat="server">
            <asp:Label CssClass="sn-iu-error" ID="ErrorLabel" runat="server" />
        </asp:PlaceHolder>
    </div>
</div>
