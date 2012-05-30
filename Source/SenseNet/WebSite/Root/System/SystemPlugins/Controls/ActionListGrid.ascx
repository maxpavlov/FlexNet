<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.ApplicationModel" %>

<asp:Panel ID="ActionListPanel" runat="server" CssClass="sn-actionlist">
    <asp:ListView ID="ActionListViewasdasd" runat="server" EnableViewState="false" >
       <LayoutTemplate>
            <ul>
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
    
    <asp:ListView ID="ActionListView" runat="server" EnableViewState="false" >
       <LayoutTemplate>
            <table class="sn-listgrid ui-widget-content">
                <thead>
                    <tr class="ui-widget-content">
                        <th class="sn-lg-col-1 ui-state-default" style="width:160px">Action</th>
                        <th class="sn-lg-col-2 ui-state-default" style="width:350px">Application path</th>
                        <th class="sn-lg-col-3 ui-state-default">Scenario(s)</th>
                        <th class="sn-lg-col-4 ui-state-default" style="width:100px"></th>
                        <th class="sn-lg-col-5 ui-state-default" style="width:100px"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr runat="server" id="itemPlaceHolder"></tr>
                </tbody>
            </table>               
       </LayoutTemplate>
       <ItemTemplate>
            <tr class="sn-lg-row0 ui-widget-content">
                <td><sn:ActionLinkButton runat="server" ID="ActionLink" /></td>
                <td><sn:ActionLinkButton runat="server" ID="AppBrowseAction" ActionName="Browse" IconVisible="false" 
                            NodePath='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' 
                            Text='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' /></td>
                <td><%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Scenario %></td>
                <td><sn:ActionLinkButton ID="ActionLinkButton1" runat="server" ActionName="CopyAppLocal" ParameterString="nodepath={CurrentContextPath}" Text="Copy local" 
                            NodePath='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' /></td>
                <td><sn:ActionLinkButton ID="ActionLinkButton2" runat="server" ActionName="DeleteLocal" Text="Delete local" 
                            NodePath='<%# ((ActionBase)Container.DataItem).GetApplication() == null ? string.Empty : ((ActionBase)Container.DataItem).GetApplication().Path %>' /></td>
            </tr>
       </ItemTemplate>
       <EmptyDataTemplate>
        No actions available
       </EmptyDataTemplate>
    </asp:ListView> 
    
</asp:Panel>