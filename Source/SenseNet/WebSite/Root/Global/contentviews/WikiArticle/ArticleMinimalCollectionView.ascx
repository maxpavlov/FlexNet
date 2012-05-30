<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
    
<div class="sn-contentlist">
 
<% var index = 0;
    foreach (var content in this.Model.Items)
  { %>
      <% if (index > 0) { %>, <% } %>
      <%=Actions.BrowseAction(content)%>
<% index++;
    } %>

</div>




