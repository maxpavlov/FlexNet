<%@ Import Namespace="System.Collections.Generic"%>
<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>

<!-- ContentView: "WebContentDemo" Browse -->
<div class="sn-content">
    
    <% if (GetValue("DisplayName") != GetValue("Name")) { %>
        <h1 class="sn-content-title"><%= GetValue("DisplayName") %></h1>
    <% } %>

	<% if (!String.IsNullOrEmpty((string)base.Content["Subtitle"])) { %>
		<h2 class="sn-content-subtitle"><%= GetValue("Subtitle") %></h2>   
	<% } %>


    <% if (!String.IsNullOrEmpty(GetValue("RelatedImage.Path")) || !String.IsNullOrEmpty(GetValue("Header"))) { %>
	<div class="sn-content-header ui-helper-clearfix">

        <% if (!String.IsNullOrEmpty(GetValue("RelatedImage.Path")))
           { %>
            <img class="sn-pic" src="<%= GetValue("RelatedImage.Path") %>" alt="" />
        <% } %>

        <% if (!String.IsNullOrEmpty(GetValue("Header"))) { %>
    		<%= GetValue("Header") %>
        <% } %>
 
	</div>
	<% } %>

    <% if (!String.IsNullOrEmpty(GetValue("Body"))) { %>
    <div class="sn-richtext ui-helper-clearfix">
        <%= GetValue("Body") %>      
    </div>
    <% } %>

    <% if (!String.IsNullOrEmpty(GetValue("Details.Href"))) { %>
    <div>
        <sn:Hyperlink ID="Details1" runat="server" FieldName="Details" RenderMode="Browse" />
    </div>
    <% } %>

</div>
