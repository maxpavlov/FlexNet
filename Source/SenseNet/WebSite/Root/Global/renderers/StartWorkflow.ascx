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
       <p>There are no workflows that you can start.</p>
    <%
        }
       else
       {
           
           var contextNode = PortalContext.Current.ContextNode;
           var contentList = ContentList.GetContentListByParentWalk(this.Model.Content.ContentHandler) as ContentList;
           var backUrl = PortalContext.Current.BackUrl;

           foreach (var content in this.Model.Items)
           {
               var addAction = ActionFramework.GetAction("StartWorkflow", SenseNet.ContentRepository.Content.Create(contentList), backUrl, new { ContentTypeName = content.Path, RelatedContent = contextNode.Path });
               
               %>

        <div class="sn-content sn-workflow ui-helper-clearfix">
            <a class="sn-actionlinkbutton" href="<%=addAction == null ? string.Empty : addAction.Uri %>">
                <img class="sn-icon sn-icon16" title="" alt="[start]" src="/Root/Global/images/icons/16/startworkflow.png" />
                Start workflow
            </a>
            <h2 class="sn-content-title">
                <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(content.Icon, null, 32)%>
                <%=content.DisplayName%> <br />
            </h2>
            <div class="sn-content-lead"><%=content.Description%></div>
        </div>
        
    <%}
       } %>
</div>
