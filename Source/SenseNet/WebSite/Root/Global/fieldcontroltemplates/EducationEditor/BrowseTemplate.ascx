<%@  Language="C#" %>

<asp:ListView ID="InnerListView" runat="server" EnableViewState="false">
    <LayoutTemplate>
        <ul>
            <li runat="server" id="itemPlaceHolder" />
        </ul>
    </LayoutTemplate>
    <ItemTemplate>
        <li><span><%# DataBinder.Eval(Container.DataItem, "SchoolName")%></span></li>
    </ItemTemplate>
    <EmptyDataTemplate>
    </EmptyDataTemplate>
</asp:ListView>
