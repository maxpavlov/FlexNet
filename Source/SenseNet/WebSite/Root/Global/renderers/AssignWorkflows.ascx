<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.ApplicationModel" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Schema" %>

<%
    var contentList = PortalContext.Current.ContextNode as ContentList;
    var backUrl = PortalContext.Current.BackUrl;
%>

<div class="sn-workflow-list">

    <%foreach (var content in this.Model.Items)
      { %>
        <%
          var typeName = content.Name.Replace(".xaml", "").Replace(".XAML", "");
          var addAction = contentList == null ? null : ActionFramework.GetAction("AssignWorkflow", SenseNet.ContentRepository.Content.Create(contentList), backUrl, new { ContentTypeName = typeName });
          var wfDefType = SenseNet.ContentRepository.Schema.ContentType.GetByName(typeName);
        %>

        <div class="sn-content sn-workflow ui-helper-clearfix">
            <div>
                <a class="sn-actionlinkbutton" href="<%=addAction == null ? string.Empty : addAction.Uri %>">
                    <img class="sn-icon sn-icon16" title="" alt="[add]" src="/Root/Global/images/icons/16/add.png" /> Assign to list
                </a>
                <h2 class="sn-content-title">
                    <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(wfDefType.Icon, null, 32)%>
                    <%=content.DisplayName %>                    
                </h2>
                <div class="sn-content-lead"><%= content["Description"] %></div>

            </div>
        </div>
        
    <%} %>
</div>
