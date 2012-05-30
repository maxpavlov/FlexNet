<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<% if (!String.IsNullOrEmpty(GetValue("Body"))) { %>
    <div class="sn-article-body sn-richtext"><%=GetValue("Body") %></div>
<% } %>
