<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<div class="sn-entries">
<% foreach (var content in this.Model.Items)
{
       if (content.Children.Count() > 0)
       {
           %><h3 style="color: red"><%
       }
       else
       {
           %><h3><%
       }%>
       <%= content.DisplayName %></h3>
       <p><%= content.Path %></p>
       <p><%= content.Description %></p>
<%} %>
</div>

