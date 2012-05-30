<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
    <br/>
    
    
    <div onclick="$('.sn-applist').toggle(); $('.sn-applist-hidebutton').toggle(); $('.sn-applist-showbutton').toggle();">
        <div class="sn-applist-hidebutton" style='<%= (this.Parent as SenseNet.Portal.Portlets.ApplicationListPresenterPortlet).IsHidden ? "display: none" : "display: block" %>'>
            <img src="/Root/Global/images/Minimize.gif" alt="Hide application list"  />&nbsp;
            Hide application list</div>
        <div class="sn-applist-showbutton" style='<%= (this.Parent as SenseNet.Portal.Portlets.ApplicationListPresenterPortlet).IsHidden ? "display: block" : "display: none" %>'>
           <img src="/Root/Global/images/Expand.gif" alt="Show application list"  />&nbsp;
           Show application list</div>
    </div>
    <br/>
    
    <%--<div class="sn-applist" style="display: none">--%>
    <div class="sn-applist" style='<%= (this.Parent as SenseNet.Portal.Portlets.ApplicationListPresenterPortlet).IsHidden ? "display: none" : "display: block" %>'>
    <asp:ListView ID="ApplicationListView" runat="server" EnableViewState="false" >
       <LayoutTemplate>
            <table class="sn-listgrid ui-widget-content">
                <thead>
                    <tr class="ui-widget-content">
                        <th class="sn-lg-col-1 ui-state-default" style="width:160px">Application name</th>
                        <th class="sn-lg-col-2 ui-state-default" style="width:350px">Location</th>
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
                <td><sn:SNIcon Icon='<%# Eval("Icon") %>' runat='server' />
                    <sn:ActionLinkButton ID="ActionLinkButton2" runat="server" ActionName="Explore" NodePath='<%# Eval("Path") %>' Text='<%# Eval("DisplayName")%>' IconVisible="false" /></td>
                <td><%# Eval("Path") %></td>
                <td><%# Eval("Scenario")%></td>
                <td><sn:ActionLinkButton runat="server" ActionName="CopyAppLocal" NodePath='<%# Eval("Path") %>' ParameterString="nodepath={CurrentContextPath}" Text="Copy local" /></td>
                <td><sn:ActionLinkButton runat="server" ActionName="DeleteLocal" NodePath='<%# Eval("Path") %>' Text="Delete local" /></td>
            </tr>
       </ItemTemplate>
       <AlternatingItemTemplate>
            <tr class="sn-lg-row1 ui-widget-content">
                <td><sn:SNIcon Icon='<%# Eval("Icon") %>' runat='server' />
                    <sn:ActionLinkButton ID="ActionLinkButton3" runat="server" ActionName="Explore" NodePath='<%# Eval("Path") %>' Text='<%# Eval("DisplayName")%>' IconVisible="false" /></td>
                <td><%# Eval("Path") %></td>
                <td><%# Eval("Scenario")%></td>
                <td><sn:ActionLinkButton runat="server" ActionName="CopyAppLocal" NodePath='<%# Eval("Path") %>' ParameterString="nodepath={CurrentContextPath}" Text="Copy local" /></td>
                <td><sn:ActionLinkButton runat="server" ActionName="DeleteLocal" NodePath='<%# Eval("Path") %>' Text="Delete local" /></td>
            </tr>
       </AlternatingItemTemplate>
       <EmptyDataTemplate>
       </EmptyDataTemplate>
    </asp:ListView>   
    
    </div>