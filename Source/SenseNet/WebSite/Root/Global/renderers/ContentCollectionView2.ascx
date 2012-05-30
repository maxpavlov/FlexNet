<%@ Control Language="C#" AutoEventWireup="true" %>


<asp:ListView ID="ContentList" runat="server" EnableViewState="false">
    <LayoutTemplate>
        <div>
            <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
        </div>
    </LayoutTemplate>
    <ItemTemplate>
        <div>
            <sn:ActionLinkButton NodePath='<%#Eval("Path") %>' 
                                 ActionName="Browse" 
                                 IconVisible="false" runat="server">
                <span><%#Eval("DisplayName") %></span>    
            </sn:ActionLinkButton>
            Created:<%#Eval("CreationDate") %>!!!
        </div>
    </ItemTemplate>
</asp:ListView>