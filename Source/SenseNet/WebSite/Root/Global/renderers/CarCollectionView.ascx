<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
  
<%foreach (var content in this.Model.Items)
  { %>
    
  <div style="float:left;">
     <img src="/Root/Global/images/icons/32/car.png" width="60" height="60" />
  </div>
  <div style="float:left; font-size:9pt; font-variant:small-caps;">
    make<span style="font-size:20pt; margin-right:10px;"><strong><%= content["Make"] %></strong></span>
    model<span style="font-size:16pt;"><%= content["Model"] %></span>
    <br/>
    style<span style="font-size:13pt;"><%= (content["Style"] as List<string>).First() %></span>
  </div>
  <div style="clear:both;"></div>
  <hr />
    
<%} %>




