<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.Portlets.ContentCollection.MyContentCollectionView" %>
<div class="sn-forum-topic">

    <asp:Repeater ID="ContentList" runat="server">
        <HeaderTemplate>
        <div class="sn-entries">
        </HeaderTemplate>
        <ItemTemplate>
            <h3><asp:Label runat="server" ID="NameLabel" Text='<%# Eval("DisplayName") %>' /></h3>
            <p><asp:Label runat="server" ID="PathLabel" Text='<%# Eval("Path") %>' /></p>
            <p><asp:Label runat="server" ID="DescriptionLabel" Text='<%# Eval("Description") %>' /></p>
        </ItemTemplate>
        <FooterTemplate>
        </div>
        </FooterTemplate>
    </asp:Repeater>

</div>
