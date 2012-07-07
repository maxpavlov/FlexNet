<%@  Language="C#" EnableViewState="false" %>
<%@ Import Namespace="SenseNet.Portal.UI.Controls" %>
<a href='<%# ((Binary)Container).Field.Content.Path %>'><%# ((Binary)Container).Field.Content.DisplayName %></a>