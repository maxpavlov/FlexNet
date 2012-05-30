<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.Controls.PortletSynchronizer" %>

<asp:Panel ID="pnlSuccess" runat="server" Visible="false">
	<b>Portlets successfully installed.</b>
	<br />
	<br />
</asp:Panel>

<asp:ListView ID="ListView1" runat="server">
    <LayoutTemplate>
        <table>
            <thead>
                <tr>
                    <th style="width:160px">Name</th>
                    <th style="width:260px">Description</th>
                    <th style="width:80px">Category</th>
                </tr>
            </thead>
            <tbody>
                <tr id="itemPlaceholder" runat="server"></tr>
            </tbody>
        </table>
    </LayoutTemplate>
    <ItemTemplate>
        <tr>
            <td><%# Eval("DisplayName")%></td>
            <td><%# Eval("Description")%></td>
            <td><%# Eval("Category")%></td>
        </tr>
    </ItemTemplate>
    <EmptyDataTemplate>
        No portlets to install.
    </EmptyDataTemplate>
</asp:ListView>

<br />
<br />
<asp:Button CssClass="sn-submit" ID="btnInstallPortlets" runat="server" Text="Install portlets" onclick="btnInstallPortlets_Click" />
<asp:Button CssClass="sn-submit" ID="btnBack" runat="server" Text="Cancel" onclick="btnBack_Click" />
