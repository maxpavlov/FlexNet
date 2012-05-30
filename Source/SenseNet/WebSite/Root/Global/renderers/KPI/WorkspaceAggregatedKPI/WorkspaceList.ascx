<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>

<div class="sn-workspace-list">

    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="left" runat="server">
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Sales" ParameterString="ContentTypeName=/Root/ContentTemplates/SalesWorkspace/Sales_Workspace;backtarget=newcontent" Text="Sales Workspace" />
            <sn:ToolbarSeparator runat="server" />    
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Project" ParameterString="ContentTypeName=/Root/ContentTemplates/ProjectWorkspace/Project_Workspace;backtarget=newcontent" Text="Project Workspace" />
            <sn:ToolbarSeparator runat="server" />    
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Document" ParameterString="ContentTypeName=/Root/ContentTemplates/DocumentWorkspace/Document_Workspace;backtarget=newcontent" Text="Document Workspace" />
        </sn:ToolbarItemGroup>
    </sn:Toolbar>


    <%foreach (var content in this.Model.Items)
      { %>
      
            <% 
          var managers = content["Manager"] as List<SenseNet.ContentRepository.Storage.Node>;
          var imgSrc = "/Root/Global/images/orgc-missinguser.png?dynamicThumbnail=1&width=64&height=64";
          var managerName = "No manager associated";
          if (managers != null) {
              var manager = managers.FirstOrDefault() as SenseNet.ContentRepository.User;
              if (manager != null) {
                  var managerC = SenseNet.ContentRepository.Content.Create(manager);
                  managerName = manager.FullName;
                  var imgField = managerC.Fields["Avatar"] as SenseNet.ContentRepository.Fields.ImageField;
                  imgField.GetData(); // initialize image field
                  var param = SenseNet.ContentRepository.Fields.ImageField.GetSizeUrlParams(imgField.ImageMode, 64, 64);
                  if (!string.IsNullOrEmpty(imgField.ImageUrl))
                      imgSrc = imgField.ImageUrl + param;
              }
          }
          %>
      
        <div class="sn-content ui-helper-clearfix" style="background-color: #FFF; border: solid 1px #DDD; padding: 10px;">
            <img style="float:right;" src="<%= imgSrc %>" title="<%= managerName %>" />
            <div style="float:right;  margin-right: 40px; text-align: right;">
                Deadline:
                <big style="font-size: 18px; display: block;"><strong><%= ((DateTime)content["Deadline"]).ToShortDateString()%></strong></big>
            </div>
            <div style="padding-right:170px">
                <h2 class="sn-content-title">
                    <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(content.Icon, null, 32) %>
                    <a href="<%=Actions.BrowseUrl(content)%>"><%=content.DisplayName %></a>
                </h2>
                <div class="sn-content-lead"><%= content["Description"] %></div>
            </div>
        </div>
        
    <%} %>

    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="left" runat="server">
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Sales" ParameterString="ContentTypeName=/Root/ContentTemplates/SalesWorkspace/Sales_Workspace;backtarget=newcontent" Text="Sales Workspace" />
            <sn:ToolbarSeparator runat="server" />    
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Project" ParameterString="ContentTypeName=/Root/ContentTemplates/ProjectWorkspace/Project_Workspace;backtarget=newcontent" Text="Project Workspace" />
            <sn:ToolbarSeparator runat="server" />    
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Document" ParameterString="ContentTypeName=/Root/ContentTemplates/DocumentWorkspace/Document_Workspace;backtarget=newcontent" Text="Document Workspace" />
        </sn:ToolbarItemGroup>
    </sn:Toolbar>

</div>

