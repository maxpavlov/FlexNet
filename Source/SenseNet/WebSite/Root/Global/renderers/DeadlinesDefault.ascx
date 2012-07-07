<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ListView"
    EnableViewState="false" %>

<asp:ListView ID="ViewBody" DataSourceID="ViewDatasource" runat="server">
    <LayoutTemplate>
        <ul class="sn-deadlines">
            <li>
                <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
            </li>
        </ul>
    </LayoutTemplate>
    <ItemTemplate>
        <asp:PlaceHolder ID="plcCalendarItem" runat="server">
            <div class="sn-deadline-daysleft">
                <strong class="<%# Eval("Task_DueCssClass") %>">
                    <%# Eval("Task_RemainingDays") %>
                </strong><span>
                    <%# Eval("Task_DueText") %></span>
            </div>
            <p class="sn-deadline-details">
                <%# Eval("GenericContent_DisplayName") %></p>
            <div>
                <a class="sn-deadline-detailslink" href='<%# SenseNet.Portal.Virtualization.PortalContext.Current.GetContentUrl(Container.DataItem) %>'>
                    Details</a></div>
        </asp:PlaceHolder>
    </ItemTemplate>
    <EmptyDataTemplate>
    </EmptyDataTemplate>
</asp:ListView>
<sn:SenseNetDataSource ID="ViewDatasource" runat="server" Top="1" />
