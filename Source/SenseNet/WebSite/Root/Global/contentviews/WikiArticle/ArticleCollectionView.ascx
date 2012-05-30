<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
    
<div class="sn-contentlist">
 
<%foreach (var content in this.Model.Items)
  { %>
    <div class="sn-content sn-contentlist-item">
        <h1 class="sn-content-title">
            <%=Actions.BrowseAction(content)%>
        </h1>
        <%= content["ModificationDate"]%>,  <%= content["ModifiedBy"]%>  
    </div>
<%} %>

</div>




