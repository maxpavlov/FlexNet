<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>

<script runat="server" type="text/C#">

public string GetNonSpecificParams()
{
    var paramlist = "";
    foreach (var key in HttpContext.Current.Request.Params.AllKeys)
    {
        var ctrl = this.Parent.FindControlRecursive(key) as TextBox;
        if (!key.Contains(Math.Abs((PortalContext.Current.ContextNode.Path + this.Model.State.Portlet.ID).GetHashCode()).ToString()) && HttpContext.Current.Request.Url.ToString().Contains(key))
        {
            if (ctrl != null && ctrl.Text != HttpContext.Current.Request.Params[key])
            {
                paramlist += "&" + key + "=" + HttpUtility.UrlEncode(ctrl.Text);
            }else
            {
                paramlist += "&" + key + "=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Params[key]);
            }
        }
    }    
    return paramlist;
}

</script>



<div style="display:none">
    <sn:ActionMenu ID="ActionMenu1" runat="server" Text="hello" NodePath="/root" Scenario="ListItem"></sn:ActionMenu>
</div>

<% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl || this.Model.VisibleFieldNames.Length > 0) { %>
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
    
    <% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl) { %>
    <div class="sn-pager">
        <%foreach (var pageAction in this.Model.Pager.PagerActions) {
        
            if (pageAction.CurrentlyActive) {  %>
                <span class="sn-pager-item sn-pager-active"><%=pageAction.PageNumber%></span>
            <% }
            else
            { %>            
                <a class="sn-pager-item" href="<%= pageAction.Url + GetNonSpecificParams() %>"><%=pageAction.PageNumber%></a>
            <% } %>
        <% } %>
    </div>
    <% } %>
</div>
<% } %>

<div class="sn-article-list sn-article-list-shortdetail">
    <ul>
    <p>Total results: <%=Model.Pager.TotalCount%></p>
    <%foreach (var content in this.Model.Items)
      { %>
            <h2 class="sn-article-title"><a href="<%=Actions.BrowseUrl(content)%>"><%=content.DisplayName %></a></h2>
    <%} %>
    </ul>
</div>

<% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl || this.Model.VisibleFieldNames.Length > 0) { %>
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
    
    <% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl) { %>
    <div class="sn-pager">
        <%foreach (var pageAction in this.Model.Pager.PagerActions) {
        
            if (pageAction.CurrentlyActive) {  %>
                <span class="sn-pager-item sn-pager-active"><%=pageAction.PageNumber%></span>
            <%} else { %>
                <a class="sn-pager-item" href="<%=pageAction.Url + GetNonSpecificParams() %>"><%=pageAction.PageNumber%></a>
            <%} %>
        
        <% } %>
    </div>
    <% } %>
</div>
<% } %>

