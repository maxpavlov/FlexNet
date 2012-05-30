<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>

<asp:Panel ID="pnlSearchControls" runat="server" DefaultButton="SearchButton" >
    <table class="sn-search">
    <tbody>
         <tr>
            <td><asp:TextBox CssClass="sn-ctrl sn-ctrl-text" ID="SearchBox" runat="server" Width="200px"/></td>
            <td>
                <asp:Button CssClass="sn-submit" ID="SearchButton" Text="Search" runat="server" />
             </td>
         </tr>
    </tbody>
    </table>  
    
    <asp:Panel runat="server" ID="ErrorPanel" Visible="false" CssClass="sn-error-msg">
        <asp:Label runat="server" ID="ErrorLabel"></asp:Label><br />
    </asp:Panel>

</asp:Panel> 

<div class="sn-search-result-count">
     <span>Result count: </span><strong><asp:Label ID="LabelResultCount" runat="server" /></strong>
</div>


<% if ((((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl && this.Model.Pager.Pagecount > 1) || this.Model.VisibleFieldNames.Length > 0)
   { %>
<div class="sn-list-navigation ui-widget-content ui-corner-all ui-helper-clearfix">
    
    <% if (this.Model.VisibleFieldNames.Length > 0) { %>
    <select class="sn-sorter sn-ctrl-select ui-widget-content ui-corner-all" onchange="if (this.value!='') window.location.href=this.value;">
        <option value="">Select ordering...</option>   
        <%foreach (var field in this.Model.VisibleFieldNames) { %>
            <option value="<%=this.Model.SortActions.First(sa => sa.SortColumn == field && sa.SortDescending == false).Url %>"><%=field %> ascending</option>
            <option value="<%=this.Model.SortActions.First(sa => sa.SortColumn == field && sa.SortDescending == true).Url %>"><%=field %> descending</option>
        <% } %>
    </select>
    <%} %>
    
    <% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl && this.Model.Pager.Pagecount > 1)
       { %>
    <div class="sn-pager">
        <%foreach (var pageAction in this.Model.Pager.PagerActions) {
        
            if (pageAction.CurrentlyActive) {  %>
                <span class="sn-pager-item sn-pager-active"><%=pageAction.PageNumber%></span>
            <%} else { %>
                <a class="sn-pager-item" href="<%=pageAction.Url %>"><%=pageAction.PageNumber%></a>
            <%} %>
        
        <% } %>
    </div>
    <% } %>
</div>
<% } %>

<div class="sn-search-results">
    <%foreach (var content in this.Model.Items)
      {
          var actionName = content.ContentHandler is User ? "Profile" : "Browse";
          %>
      
        <div class="sn-search-result ui-helper-clearfix">
            <div style="float:left; padding:3px 10px 3px 10px;">
              <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(content.Icon, null, 32) %>
            </div>            
            <div style="padding:3px 0 5px 0;">               
              <a href="<%=Actions.ActionUrl(content, actionName)%>"><%=content.DisplayName %></a>
              <br/>
              <%= content.ContentHandler.ParentId == 0 ? string.Empty : content.ContentHandler.ParentPath %>
            </div>
        </div>
        
    <%} %>
</div>

<% if ((((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl && this.Model.Pager.Pagecount > 1) || this.Model.VisibleFieldNames.Length > 0)
   { %>
<div class="sn-list-navigation ui-widget-content ui-corner-all ui-helper-clearfix">
    
    <% if (this.Model.VisibleFieldNames.Length > 0) { %>
    <select class="sn-sorter sn-ctrl-select ui-widget-content ui-corner-all" onchange="if (this.value!='') window.location.href=this.value;">
        <option value="">Select ordering...</option>   
        <%foreach (var field in this.Model.VisibleFieldNames) { %>
            <option value="<%=this.Model.SortActions.First(sa => sa.SortColumn == field && sa.SortDescending == false).Url %>"><%=field %> ascending</option>
            <option value="<%=this.Model.SortActions.First(sa => sa.SortColumn == field && sa.SortDescending == true).Url %>"><%=field %> descending</option>
        <% } %>
    </select>
    <%} %>
    
    <% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl && this.Model.Pager.Pagecount > 1)
       { %>
    <div class="sn-pager">
        <%foreach (var pageAction in this.Model.Pager.PagerActions) {
        
            if (pageAction.CurrentlyActive) {  %>
                <span class="sn-pager-item sn-pager-active"><%=pageAction.PageNumber%></span>
            <%} else { %>
                <a class="sn-pager-item" href="<%=pageAction.Url %>"><%=pageAction.PageNumber%></a>
            <%} %>
        
        <% } %>
    </div>
    <% } %>
</div>
<% } %>


    