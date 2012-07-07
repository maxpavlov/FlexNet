<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
   
<ul class="sn-menu">
<% var index = 1;
  foreach (var content in this.Model.Items.Where(item => !(bool)item["Hidden"]))
  { %>
          <li class='<%="sn-menu-" + index++ %>'>
            <a href="<%= Actions.BrowseUrl(content) %>"><%= content.DisplayName %></a>
          </li>
<%} %>
</ul>