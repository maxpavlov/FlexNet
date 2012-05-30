<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
   
<% if (this.Model.Items.Count() > 0) { %>

<ul class="sn-list">
<%foreach (var content in this.Model.Items) { %>
    <li><a href="<%= content["Url"] %>"><%= content.DisplayName %></a></li>
<%} %>
</ul>
<% } else { %>
<div class="sn-warning-msg ui-widget-content ui-state-default" style="padding: 5px;">The list is empty…</div>
<% } %>




