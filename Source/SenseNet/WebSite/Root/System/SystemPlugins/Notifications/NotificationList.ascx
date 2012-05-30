<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>

<sn:ContextInfo runat="server" ID="ContextInfoUser" Selector="CurrentUser" />
<% var targetUser = (ContextBoundPortlet.GetContainingContextBoundPortlet(this) as NotificationListPortlet).User; %>

<div class="sn-content-inlineview-header ui-helper-clearfix">
    <%= SenseNet.Portal.UI.IconHelper.RenderIconTag("user", null, 32)%>
    <div class="sn-content-info">
        <h2 class="sn-view-title"><%= targetUser.FullName %></h2>
        <strong></strong> <%= targetUser.Username %>
    </div>
</div>

<asp:Panel ID="pnlNotification" runat="server" CssClass="sn-notification" >
    <asp:ListView ID="ContentList" runat="server" EnableViewState="false">
        <LayoutTemplate>
            <table class="sn-notification-table sn-calendar">
                <tr>
                    <th runat="server"><%= GetGlobalResourceObject("Notification", "ContentName")%></th>
                    <th runat="server"><%= GetGlobalResourceObject("Notification", "ContentPath")%></th>
                    <th runat="server"><%= GetGlobalResourceObject("Notification", "Frequency")%></th>
                    <th runat="server" colspan="3"><%= GetGlobalResourceObject("Notification", "Actions")%></th>
                </tr>
                <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
            </table>
        </LayoutTemplate>
        <ItemTemplate>
            <tr class='<%# (bool)((SenseNet.ContentRepository.Content)Container.DataItem)["IsActive"] ? "sn-active" : "sn-deactive" %>' runat="server">
                <td>
                    <%# (SenseNet.ContentRepository.Content.Load(Eval("ContentPath").ToString())).DisplayName %>
                </td>
                <td><span><%# Eval("ContentPath") %></span></td>
                <td><span class="sn-frequency"><%# Eval("Frequency") %></span></td>
                <td>
                    <sn:ActionLinkButton runat="server" ID="BtnSetActivation" CssClass="sn-link" ActionName="SetActivation" ContextInfoID="ContextInfoUser" IconVisible="false" 
                        ParameterString='<%# "ContentPath=" + Eval("ContentPath") + ";IsActive=" + Eval("IsActive") + ";UserPath=" + Eval("UserPath") %>'>
                        <%# (bool)((SenseNet.ContentRepository.Content)Container.DataItem)["IsActive"]
                            ? GetGlobalResourceObject("Notification", "Deactivate")
                            : GetGlobalResourceObject("Notification", "Activate")%>
                    </sn:ActionLinkButton>
                </td>
                <td>
                    <sn:ActionLinkButton runat="server" ID="BtnEdit" CssClass="sn-link" ActionName="Notification" IconVisible="false" NodePath='<%#Eval("ContentPath") %>' 
                        ParameterString='<%# "ContentPath=" + Eval("ContentPath") + ";UserPath=" + Eval("UserPath") %>'>
                        <%= GetGlobalResourceObject("Notification", "Edit")%>
                    </sn:ActionLinkButton>
                </td>
                <td>
                    <sn:ActionLinkButton runat="server" ID="BtnDelete" CssClass="sn-link" ActionName="DeleteNotification" IconVisible="false" NodePath='<%#Eval("ContentPath") %>'
                        ParameterString='<%# "UserPath=" + Eval("UserPath") %>'>
                        <%= GetGlobalResourceObject("Notification", "Delete")%>
                    </sn:ActionLinkButton>
                </td>
            </tr>
        </ItemTemplate>
        <EmptyDataTemplate>
            <tr>
                <td>
                    <span><%= GetGlobalResourceObject("Notification", "NoSubscriptions")%></span>
                </td>
            </tr>
        </EmptyDataTemplate>
    </asp:ListView>
    <sn:BackButton ID="BtnBack" runat="server" Text="Back" CssClass="sn-submit" style="float: right;" />
</asp:Panel>