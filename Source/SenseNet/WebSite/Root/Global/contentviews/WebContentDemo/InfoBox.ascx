<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<div class="sn-article-content sn-infobox">
   
    <% if (GetValue("DisplayName") != GetValue("Name")) { %><h1 class="sn-content-title sn-article-title"><%=GetValue("DisplayName") %></h1><% } %>
    <% if (!String.IsNullOrEmpty(GetValue("Subtitle"))) { %><h3 class="sn-content-subtitle sn-article-subtitle"><%=GetValue("Subtitle") %></h3><% } %>
    
    <% if (!String.IsNullOrEmpty(GetValue("Header"))) { %><div class="sn-article-lead sn-richtext"><%=GetValue("Header") %></div><% } %>
    <% if (!String.IsNullOrEmpty(GetValue("Body"))) { %><div class="sn-article-body sn-richtext"><%=GetValue("Body") %></div><% } %>

</div>