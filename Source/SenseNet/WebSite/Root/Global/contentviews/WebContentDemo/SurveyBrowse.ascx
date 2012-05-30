<%@ Import Namespace="System.Collections.Generic"%>
<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>

<div class="sn-content">

	<% if (!String.IsNullOrEmpty((string)base.Content["Subtitle"])) { %>
		<h2 class="sn-content-subtitle"><%= GetValue("Subtitle") %></h2>   
	<% } %>

    <% if (!String.IsNullOrEmpty(GetValue("Body"))) { %>
    <div class="sn-richtext ui-helper-clearfix">
        <%= GetValue("Body") %>      
    </div>
    <% } %>

</div>
