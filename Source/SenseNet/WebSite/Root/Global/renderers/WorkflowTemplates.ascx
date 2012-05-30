<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.ApplicationModel" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>


<div class="sn-workflow-list">

    <% if (this.Model.Content == null || this.Model.Content.ChildCount == 0)
       { %>
       There are no workflows assigned to this content list. Please select one from the available workflow definitions.
    <%
        }
       else
       {
           foreach (var content in this.Model.Items)
           { 
              var typeName = content.Name.Replace(".xaml", "").Replace(".XAML", "");
              var deleteAction = ActionFramework.GetAction("Delete", content, PortalContext.Current.BackUrl, null);
            %>


        <div class="sn-content sn-workflow ui-helper-clearfix">
                <% if (deleteAction != null) { %>
                <a class="sn-actionlinkbutton" href="<%=deleteAction.Uri %>">
                    <%= SenseNet.Portal.UI.IconHelper.RenderIconTag("delete", null, 16)%> Remove from list
                </a>
                <% } %>
                <h2 class="sn-content-title">
                    <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(content.Icon, null, 32)%>
                    <%= Actions.BrowseAction(content) %>    
                </h2>
                <div class="sn-content-lead"><%= content["Description"] %></div>
        </div>
        
    <%}
       } %>
</div>
