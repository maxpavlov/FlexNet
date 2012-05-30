<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="System.Collections.Generic"%>
<%@ Import namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>

<div class="sn-content">
    <%= this.Content.Fields["Description"].GetData() %>
</div>
<div class="sn-panel ui-widget-content">
    <a class="sn-floatright" href='<%= SenseNet.Portal.PortletFramework.PortalActionLinkResolver.Instance.ResolveRelative(this.Content.Path, "RSS") %>'>
        <sn:SNIcon ID="SNIcon1" Icon="rss" runat="server" />RSS feed
    </a>
    <strong>Project is active:</strong> <%= this.Content.Fields["IsActive"].GetData() %> <span class="sn-separator">|</span>
    <strong>Completion:</strong> <%= this.Content.Fields["Completion"].GetData() %>%  
</div>
