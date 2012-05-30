<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true"
    ID="myContext" />
<div class="sn-content sn-event">
    <% if (Security.IsInRole("Editors"))
       { %>
    <div class="sn-content-actions">
        <sn:ActionLinkButton ID="ActionLinkButton1" runat="server" ActionName="Edit" Text="Edit"
            ContextInfoID="myContext" />
    </div>
    <%} %>
    <% if (!String.IsNullOrEmpty(GetValue("DisplayName")))
       { %><h1 class="sn-content-title">
        <%=GetValue("DisplayName") %></h1>
    <% } %>
    <div class="sn-content-info">
        <dl>
            <dt><strong>Location:</strong></dt><dd><%=GetValue("Location") %></dd>
            <dt><strong>Start date:</strong></dt><dd><%=GetValue("StartDate")%></dd>
            <dt><strong>End date:</strong></dt><dd><%=GetValue("EndDate")%></dd>
            <% if (!String.IsNullOrEmpty(GetValue("EventUrl"))) { %><dt><strong>Further information:</strong></dt><dd><a href="<%=GetValue("EventUrl") %>"><%=GetValue("EventUrl")%></a></dd><% } %>
        </dl>
    </div>
    <div class="sn-content-lead">
        <%=GetValue("Lead") %>
    </div>
    <div class="sn-content-body">
        <%=GetValue("Description")%>
    </div>

    <div class="sn-panel sn-buttons">
        <sn:BackButton CssClass="sn-submit" Text="Back" ID="BackButton1" runat="server" />
    </div>
</div>
