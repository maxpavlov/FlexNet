<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Register TagPrefix="sn" Assembly="SenseNet.CorePortlets" Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Data" %>
<asp:ListView ID="ContentList" runat="server" EnableViewState="false">
    <LayoutTemplate>
        <table>
            <tr class="sn-eventmanager-header">
                <th>
                    <asp:Label ID="Label0" runat="server"><%=SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "UserName") %></asp:Label>
                </th>
                <th>
                    <asp:Label ID="Label1" runat="server"><%=SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "SubscriptionDate") %></asp:Label>
                </th>
                <th>
                    <asp:Label ID="Label2" runat="server"><%=SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "Participants") %></asp:Label>
                </th>
                <th>
                    <asp:Label ID="Label3" runat="server"><%=SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "Approving") %></asp:Label>
                </th>
                <th>
                    <asp:Label ID="Label4" runat="server"><%=SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "Delete") %></asp:Label>
                </th>
            </tr>
            <tbody class="sn-eventmanager-body">
                <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
            </tbody>
        </table>
    </LayoutTemplate>
    <ItemTemplate>
        <div>
            <tr>
                <td>
                    <span class="sn-eventmanager-createdby"><%#Eval("CreatedBy") %></span>
                </td>
                <td>
                    <span class="sn-eventmanager-creationdate"><%#Eval("CreationDate") %></span>
                </td>
                <td>
                    <span class="sn-eventmanager-participants"><%#((int)Eval("GuestNumber") + 1).ToString() %></span>
                </td>
                <td>
                    <asp:PlaceHolder ID="PlaceHolder1" runat="server" Visible='<%#Eval("Version").ToString().Contains("A") %>'>
                        <span class="sn-eventmanager-approved"><%# SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "Approved")%></span>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder ID="PlaceHolder2" runat="server" Visible='<%#Eval("Version").ToString().Contains("P") %>'>
                        <span class="sn-eventmanager-approve"><a href="<%#Eval("Path") %>?action=Approve&back=<%=SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri %>">
                            <%# SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "Approve")%></a></span>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder ID="PlaceHolder3" runat="server" Visible='<%#Eval("Version").ToString().Contains("R") %>'>
                        <span class="sn-eventmanager-rejected"><%# SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "Rejected")%></span>
                    </asp:PlaceHolder>
                </td>
                <td>
                    <span class="sn-eventmanager-delete"><a href="<%#Eval("Path") %>?action=Cancel&back=<%=SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri %>">
                        <%# SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("EventManager", "Delete")%></a></span>
                </td>
            </tr>
        </div>
    </ItemTemplate>
</asp:ListView>
​