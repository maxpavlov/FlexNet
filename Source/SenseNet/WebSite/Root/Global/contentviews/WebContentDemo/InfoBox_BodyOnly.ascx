<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>

<% if (!String.IsNullOrEmpty(GetValue("Body"))) { %>
<div class="sn-article-content sn-infobox">
    <div class="sn-article-body sn-richtext"><%=GetValue("Body") %></div>
</div>
<% } %>
