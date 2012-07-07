<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>

<sn:ContextInfo runat="server" ID="newWS" />

<% string user = (SenseNet.ContentRepository.User.Current).ToString(); %>
<%if (user == "Visitor")
  {%>
   <div class="sn-pt-body-border ui-widget-content ui-corner-all">
	<div class="sn-pt-body ui-corner-all">
		<%=GetGlobalResourceObject("Portal", "WSContentList_Visitor")%>
	</div>
</div>
<% }%>
<%else
  {%>

<div class="sn-workspace-list">

    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="left" runat="server">
            <sn:ActionMenu ID="ActionMenu1" runat="server" Scenario="New" ContextInfoID="myContext" RequiredPermissions="AddNew" CheckActionCount="True">
                <sn:ActionLinkButton ID="ActionLinkButton1" runat="server" ActionName="Add" IconUrl="/Root/Global/images/icons/16/newfile.png" ContextInfoID="newWS" Text="New" CheckActionCount="True"  ParameterString="backtarget=newcontent"/>
            </sn:ActionMenu>   
        </sn:ToolbarItemGroup>
    </sn:Toolbar>

    <% foreach (var content in this.Model.Items) { %>
      
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
      
        <div class="sn-content ui-helper-clearfix" style="margin-bottom: 10px; background-color: #FFF; border: solid 1px #DDD; padding: 10px;">
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
</div>

<%} %>