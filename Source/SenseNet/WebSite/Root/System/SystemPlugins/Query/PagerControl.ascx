<%@ Control Language="C#" AutoEventWireup="true"  Inherits="SenseNet.Portal.UI.Controls.PagerControl" %>
<asp:Repeater ID="PageRepeater" runat="server" OnItemDataBound="Repeater_OnItemDataBound" OnItemCommand="Repeater_OnItemCommand">
    <HeaderTemplate>
        <asp:LinkButton ID="lnkFirst" CssClass="SNPager_FirstPage" runat="server" CommandName="PageFirst" Text='<%$ Resources: PagerControl, FirstPage %>' />
        <asp:Label ID="lnkFirstDisabled" CssClass="SNPager_FirstPage_Disabled" runat="server" Text='<%$ Resources: PagerControl, FirstPage %>' Visible ="false" />
        &nbsp;&nbsp;
        <asp:LinkButton ID="lnkBackMore" CssClass="SNPager_BackMorePage" runat="server" CommandName="PageBackMore" Text='<%$ Resources: PagerControl, BackMorePages %>' />
        <asp:Label ID="lnkBackMoreDisabled" CssClass="SNPager_BackMorePage_Disabled" runat="server" Text='<%$ Resources: PagerControl, BackMorePages %>' Visible ="false" />
        &nbsp;&nbsp;
        <asp:LinkButton ID="lnkBack" CssClass="SNPager_BackPage" runat="server" CommandName="PageBack" Text='<%$ Resources: PagerControl, BackPage %>' />
        <asp:Label ID="lnkBackDisabled" CssClass="SNPager_BackPage_Disabled" runat="server" Text='<%$ Resources: PagerControl, BackPage %>' Visible ="false" />
        &nbsp;&nbsp;
        <asp:PlaceHolder ID="InvisiblePagesBefore" runat="server"> ... </asp:PlaceHolder>
    </HeaderTemplate>
    <ItemTemplate>
        &nbsp;
        <asp:LinkButton ID="lnkPage" CssClass="SNPager_Page" CommandName="PageSelected" CommandArgument='<%# Container.DataItem %>' runat="server" Text='<%# Container.DataItem %>' />
        <asp:Label ID="lnkPageDisabled" CssClass="SNPager_Page_Disabled" runat="server" Text='<%# Container.DataItem %>' Visible ="false" />
        &nbsp;
    </ItemTemplate>
    <FooterTemplate>
        <asp:PlaceHolder ID="InvisiblePagesAfter" runat="server"> ... </asp:PlaceHolder>
        &nbsp;&nbsp;
        <asp:LinkButton ID="lnkForward" CssClass="SNPager_ForwardPage" CommandName="PageForward" runat="server" Text='<%$ Resources: PagerControl, ForwardPage %>' />
        <asp:Label ID="lnkForwardDisabled" CssClass="SNPager_ForwardPage_Disabled" runat="server" Text='<%$ Resources: PagerControl, ForwardPage %>' Visible ="false" />
        &nbsp;&nbsp;
        <asp:LinkButton ID="lnkForwardMore" CssClass="SNPager_ForwardMorePage" CommandName="PageForwardMore" runat="server" Text='<%$ Resources: PagerControl, ForwardMorePages %>' />
        <asp:Label ID="lnkForwardMoreDisabled" CssClass="SNPager_ForwardMorePage_Disabled" runat="server" Text='<%$ Resources: PagerControl, ForwardMorePages %>' Visible ="false" />
        &nbsp;&nbsp;
        <asp:LinkButton ID="lnkLast" CssClass="SNPager_LastPage" runat="server" CommandName="PageLast" Text='<%$ Resources: PagerControl, LastPage %>' />
        <asp:Label ID="lnkLastDisabled" CssClass="SNPager_LastPage_Disabled" runat="server" Text='<%$ Resources: PagerControl, LastPage %>' Visible ="false" />
    </FooterTemplate>
</asp:Repeater>
&nbsp;&nbsp;&nbsp;&nbsp;

<asp:PlaceHolder ID="PageSizePanel" runat="server" >
    <b>Page size: </b>
    <asp:DropDownList ID="PageSizeListControl" runat="server" AutoPostBack="true" OnSelectedIndexChanged="PageSizeListControl_SelectedIndexChanged">
        <asp:ListItem Text="2" Value="2" Selected="True" />
        <asp:ListItem Text="5" Value="5" />
        <asp:ListItem Text="10" Value="10" />
        <asp:ListItem Text="50" Value="50" />
    </asp:DropDownList>
</asp:PlaceHolder>

&nbsp;&nbsp
<b>Total result count: </b><%= this.ResultCount.ToString() %>
