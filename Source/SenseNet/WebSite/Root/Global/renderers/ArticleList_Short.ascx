<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<div class="sn-article-list sn-article-list-short">
    <%foreach (var content in this.Model.Items)
      { %>
      
        <div class="sn-article-list-item">
            <%if (Security.IsInRole("Editors")) { %>
            <div><%= Actions.ActionMenu(content.Path, "Manage Content", "ListItem")%></div>
            <%} %>
            <h2 class="sn-article-title"><a href="<%=Actions.BrowseUrl(content)%>"><%=content.DisplayName %></a></h2>
            <small class="sn-article-info"><%= content["ModificationDate"]%></small>
            <div class="sn-article-lead">
                <%=content["Lead"] %>
            </div>
        </div>
        
    <%} %>
</div>
<div style="display:none">
    <sn:ActionMenu ID="ActionMenu1" runat="server" Text="hello" NodePath="/root" Scenario="ListItem"></sn:ActionMenu>
</div>