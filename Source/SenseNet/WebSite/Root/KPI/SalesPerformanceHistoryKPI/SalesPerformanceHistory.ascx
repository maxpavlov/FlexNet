<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>

<%  
    var portlet = this.Parent as GlobalKPIPortlet;
    var kpiDS = portlet.ContextNode as KPIDatasource; 
    
    var actualMax = kpiDS.KPIDataList.Max(d => d.Actual);
    var goalMax = kpiDS.KPIDataList.Max(d => d.Goal);
    var max = actualMax > goalMax ? actualMax : goalMax;
%>

<div>
<% foreach (var kpiData in kpiDS.KPIDataList) { %>

    <div class="sn-kpi-history">
        <h3><%= kpiData.Label%></h3>
        <div class="sn-progress sn-kpi-goal">
            <span style="width:<%= ((double)kpiData.Goal / (double)max * 100).ToString("N") %>%"></span>
            <em>Goal: <%=kpiData.Goal.ToString()%></em>
        </div>
        <div class="sn-progress sn-kpi-actual">
            <span style="width:<%= ((double)kpiData.Actual / (double)max * 100).ToString("N") %>%"></span>
            <em>Actual: <%=kpiData.Actual.ToString()%></em>
        </div>
    </div>

<% } %>     
</div>
