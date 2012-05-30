<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<asp:Panel ID="ActionListPanel" runat="server" CssClass="sn-actionlist">
    <asp:ListView ID="ActionListView" runat="server" EnableViewState="false" >
       <LayoutTemplate>
            <ul class="ui-helper-reset">
                <li runat="server" id="itemPlaceHolder"></li>
            </ul>               
       </LayoutTemplate>
       <ItemTemplate>
            <li>
                <sn:ActionLinkButton runat="server" ID="ActionLink" />
            </li>
       </ItemTemplate>
       <EmptyDataTemplate>
       </EmptyDataTemplate>
    </asp:ListView>   
</asp:Panel>