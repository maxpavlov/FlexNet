<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>

<sn:ContextInfo runat="server" ID="newWS" />

<div class="sn-workspace-list">

    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="left" runat="server">
            <sn:ActionLinkButton runat="server" ActionName="Add" ContextInfoID="newWS" Text="new Workspace" ParameterString="backtarget=newcontent" />
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

          var nowDate = DateTime.Now;
          DateTime resDate;
          DateTime? lastDate = null;
          DateTime? deadLine = null;
          int elapsed = 0;
          var portlet = this.Parent as WorkspaceSummaryPortlet;
          int mediumLimit = portlet == null ? 5 : portlet.DaysMediumWarning;
          int strongLimit = portlet == null ? 20 : portlet.DaysHighWarning;

          if (DateTime.TryParse(content["ModificationDate"].ToString(), out resDate))
              lastDate = resDate;
          if (DateTime.TryParse(content["Deadline"].ToString(), out resDate))
              deadLine = resDate;
          
          // calc days from last modification
          int progressIndication = 0;
          if (lastDate.HasValue) {
              elapsed = new TimeSpan(nowDate.Ticks - lastDate.Value.Ticks).Days;
              if (elapsed <= mediumLimit)
                  progressIndication = 1;
              else if (elapsed <= strongLimit)
                  progressIndication = 2;
              else
                  progressIndication = 3;
          }
          
          %>
      
        <div class="sn-ws-listitem sn-ws-sales sn-ws-activity ui-widget ui-widget-content ui-corner-all ui-helper-clearfix">
            <div class="sn-ws-info">
                Responsible:<br />
                <img class="sn-pic sn-pic-center" src="<%= imgSrc %>" alt="<%= managerName %>" title="<%= managerName %>" /><br />
                <strong><%= managerName %></strong><br />
            </div>
            <div class="sn-layoutgrid ui-helper-clearfix">
                <div class="sn-layoutgrid-column sn-layoutgrid1">
                    <h2 class="sn-content-title">
                        <a href="<%=Actions.BrowseUrl(content)%>"><%=content.DisplayName %></a>
                    </h2>
                     <div class="sn-content-lead">
                        <%= content["Description"] %>
                    </div>
                </div>
                <div class="sn-layoutgrid-column sn-layoutgrid2">
                    <div class="sn-ws-lastmodify">
                        <div>Deadline: <big><%= ((DateTime)content["Deadline"]).ToString("dd.MM.yyyy HH:mm")%></big></div>
                        <div>Modified:
                            <big class="sn-kpi-lastmod-<%= progressIndication %>"><%= ((DateTime)content["ModificationDate"]).ToString("dd.MM.yyyy HH:mm")%></big> <small>(<%= elapsed %> day(s) ago)</small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
    <%} %>
</div>

