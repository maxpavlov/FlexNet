<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>

<div class="sn-workspace-list">

    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="left" runat="server">
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Sales" ParameterString="ContentTypeName=/Root/ContentTemplates/SalesWorkspace/Sales_Workspace" Text="Sales Workspace" />
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
          double? maxRevenue = 200000000;
          double? chanceToWin = null;
          double? expectedRevenue = null;
          double? progress = null;
          double res;

          var nowDate = DateTime.Now;
          DateTime resDate;
          DateTime? startDate = null;
          DateTime? deadLine = null;
          
          if (Double.TryParse(content["ChanceOfWinning"].ToString(), out res))
              chanceToWin = res;
          if (Double.TryParse(content["Completion"].ToString(), out res))
              progress = res;
          if (Double.TryParse(content["ExpectedRevenue"].ToString(), out res))
              expectedRevenue = res;
          if (DateTime.TryParse(content["StartDate"].ToString(), out resDate))
              startDate = resDate;
          if (DateTime.TryParse(content["Deadline"].ToString(), out resDate))
              deadLine = resDate;
          
          // calc deadline progress
          int progressIndication = 0;
          if (startDate.HasValue && deadLine.HasValue && progress.HasValue) {
              var elapsed = new TimeSpan(nowDate.Ticks - startDate.Value.Ticks).Days;
              var remaining = new TimeSpan(deadLine.Value.Ticks - startDate.Value.Ticks).Days;
              var dateProgress = (double)elapsed / (double)remaining;
              var overallProgress = dateProgress - (progress.Value / 100);
              if (overallProgress <= 0)
                  progressIndication = 1;
              else if (overallProgress <= 0.2)
                  progressIndication = 2;
              else
                  progressIndication = 3;
          }
          
          %>
      
        <div class="sn-ws-listitem sn-ws-sales ui-widget ui-widget-content ui-corner-all ui-helper-clearfix">
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
                        <div class="sn-event-schedule">
                            <span class="sn-event-date sn-event-start">
                                <small class="sn-event-year"><%= ((DateTime)content["Deadline"]).Year%></small> 
                                <small class="sn-event-month"><%= ((DateTime)content["Deadline"]).ToString("MMM")%></small> 
                                <big class="sn-event-day"><%= ((DateTime)content["Deadline"]).Day%></big> 
                            </span>
                        </div>
                        <%= content["Description"] %>
                    </div>
                </div>
                <div class="sn-layoutgrid-column sn-layoutgrid2">
                    <div class="sn-kpi-light sn-kpi-light-<%= progressIndication.ToString() %>"></div>
                    <strong>Expected Revenue:</strong><div class="sn-progress sn-kpi-revenue"><span style="width:<%= expectedRevenue.HasValue ? (expectedRevenue.Value/maxRevenue.Value*100).ToString() : "0"%>%"><%= expectedRevenue.HasValue ? expectedRevenue.Value.ToString() : "?"%>&curren;</span></div>
                    <strong>Chance to win:</strong><div class="sn-progress sn-kpi-chance"><span style="width:<%= chanceToWin.HasValue ? chanceToWin.Value.ToString() : "0"%>%"><%= chanceToWin.HasValue ? chanceToWin.Value.ToString() : "0"%>%</span></div>
                    <strong>Progress:</strong><div class="sn-progress sn-kpi-progress"><span style="width:<%= progress.HasValue ? progress.Value.ToString() : "0"%>%"><%= progress.HasValue ? progress.Value.ToString() : "0"%>%</span></div>
                </div>
            </div>
        </div>
        
    <%} %>

    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="left" runat="server">
            <sn:ActionLinkButton runat="server" ActionName="Add" NodePath="/Root/Sites/Default_Site/workspaces/Sales" ParameterString="ContentTypeName=/Root/ContentTemplates/SalesWorkspace/Sales_Workspace" Text="Sales Workspace" />
        </sn:ToolbarItemGroup>
    </sn:Toolbar>

</div>

