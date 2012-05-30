<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

    <asp:ListView ID="ActionListView" runat="server" EnableViewState="false" >
       <LayoutTemplate>
                <asp:PlaceHolder runat="server" id="itemPlaceHolder"></asp:PlaceHolder>         
       </LayoutTemplate>
       <ItemTemplate>
                <sn:ActionLinkButton runat="server" ID="ActionLink" />
       </ItemTemplate>
       <EmptyDataTemplate>
       </EmptyDataTemplate>
    </asp:ListView>   