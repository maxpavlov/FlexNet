<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>

<%
    var portlet = this.Parent as GlobalKPIPortlet;
    var kpiDS = portlet.ContextNode as KPIDatasource; 
%>

<table class="sn-kpi-meter">
    <thead>
        <tr>
            <th>&nbsp;</th>
            <% foreach (var kpiData in kpiDS.KPIDataList) { %><th><%= kpiData.Label%></th><% } %>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>&nbsp;</td>
            <% foreach (var kpiData in kpiDS.KPIDataList) {
                   
                   int pic = 0;
                   double percent = ((double)kpiData.Actual / (double)kpiData.Goal * 100);
                   if (percent < 40)
                           pic = 10;
                       else if (percent < 50)
                           pic = 40;
                       else if (percent < 90)
                           pic = 50;
                       else
                           pic = 90;
            %>
            <td class="sn-kpi-gauge">
                <img src="/Root/Global/images/gauge<%= pic %>.gif" alt="<%= percent %>%" title="<%= percent %>%"/>
            </td>
            <% } %>
        </tr>
        <tr>
            <th>Goal</th>
            <% foreach (var kpiData in kpiDS.KPIDataList) { %><td><%=kpiData.Goal.ToString()%></td><% } %>
        </tr>
        <tr>
            <th>Actual</th>
            <% foreach (var kpiData in kpiDS.KPIDataList) { %><td><%=kpiData.Actual.ToString()%></td><% } %>
        </tr>
    </tbody>
</table>
