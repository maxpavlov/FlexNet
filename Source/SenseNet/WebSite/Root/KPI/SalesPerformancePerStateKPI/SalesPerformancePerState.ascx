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

<table class="sn-kpi-states">
    <tbody>
        <tr>
            <td>&nbsp;</td>
            <% foreach (var kpiData in kpiDS.KPIDataList) { %>
            <td>
                <span class="sn-progress sn-progress-vert sn-kpi-goal"><span style="height:<%= ((double)kpiData.Goal / (double)max * 100).ToString("N") %>%" title="Goal:<%=kpiData.Goal.ToString()%>"><%=kpiData.Goal.ToString()%></span></span>
                <span class="sn-progress sn-progress-vert sn-kpi-actual"><span style="heigth:<%= ((double)kpiData.Actual / (double)max * 100).ToString("N") %>%" title="Actual:<%=kpiData.Actual.ToString()%>"><%=kpiData.Actual.ToString()%></span></span>
            </td>
            <% } %>
        </tr>
        <tr>
            <th class="sn-kpi-goal">Goal</th>
            <% foreach (var kpiData in kpiDS.KPIDataList) { %><td><%=kpiData.Goal.ToString()%></td><% } %>
        </tr>
        <tr>
            <th class="sn-kpi-actual">Actual</th>
            <% foreach (var kpiData in kpiDS.KPIDataList) { %><td><%=kpiData.Actual.ToString()%></td><% } %>
        </tr>
    </tbody>
    <tfoot>
        <tr>
            <th>&nbsp;</th>
            <% foreach (var kpiData in kpiDS.KPIDataList) { %><th><%= kpiData.Label%></th><% } %>
        </tr>
    </tfoot>
</table>
