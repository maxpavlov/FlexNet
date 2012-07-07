<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>

<% 
    var context = SenseNet.Portal.Virtualization.PortalContext.Current.ContextWorkspace;
    double? chanceToWin = null;
    double? expectedRevenue = null; 
    double res;
    
    if (Double.TryParse(context["ChanceOfWinning"].ToString(), out res))
        chanceToWin = res;
    if (Double.TryParse(context["ExpectedRevenue"].ToString(), out res))
        expectedRevenue = res;

%>


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
<div class="sn-kpi-chance2winnum">
    <big><%= chanceToWin.HasValue ? chanceToWin.Value.ToString() : "0"%>%</big>
    <small>Expected revenue: <strong><%= expectedRevenue.HasValue ? expectedRevenue.Value.ToString("N0") : "?"%>&curren;</strong></small>
</div>

<%} %>