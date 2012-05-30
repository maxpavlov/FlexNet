<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<sn:CssRequest ID="CssRequest1" CSSPath="$skin/styles/prettyPhoto.css" runat="server" />
<sn:ScriptRequest runat="server" Path="$skin/scripts/jquery/jquery.js" />
<sn:ScriptRequest runat="server" Path="$skin/scripts/jquery/plugins/jquery.prettyPhoto.js" />
<sn:InlineScript runat="server">
<script type="text/javascript" charset="utf-8">
    $(document).ready(function() {
        $(".sn-gallery a[rel^='prettyPhoto']").prettyPhoto({
            theme: 'facebook',
            deeplinking: false,
            overlay_gallery: false
        });
    });
</script>
</sn:InlineScript>

<%  
    string galleryTitle = this.Model.Content.DisplayName;
    string galleryDesc = this.Model.Content.Description;
    string thumbX = this.Model.Content["ThumbSizeX"].ToString();
    string thumbY = this.Model.Content["ThumbSizeY"].ToString();
    string viewX = this.Model.Content["ViewSizeX"].ToString();
    string viewY = this.Model.Content["ViewSizeY"].ToString();
%>

<div class="sn-gallery">
       
    <% if (!String.IsNullOrEmpty(galleryTitle)) { %>
        <h2 class="sn-gallery-title"><%= galleryTitle %></h2>       
    <% } %>
    
    <% if (!String.IsNullOrEmpty(galleryDesc))
       { %>
    <div class="sn-gallery-description">
        <%= galleryDesc %>
    </div>
    <% } %>

    <div class="sn-gallery-thumbs ui-helper-clearfix">

        <% foreach (var content in this.Model.Items)
           { %>
            <div class="sn-gallery-item">
               <div class="sn-gallery-thumb" style="width:<%=thumbX%>px;height:<%=thumbY%>px;line-height:<%=thumbY%>px;">

                <%-- 
                    "Background image" view
                    ----------------
                    In this layout the view sized images appears as the centered background image of the thumbnail links.
                --%>

                    <a href="<%=string.Format("/binaryhandler.ashx?nodeid={0}&propertyname=Binary&width={1}&height={2}", content.Id, viewX, viewY) %>" rel="prettyPhoto[gallery1]" title="<%=content["Description"]%>">
                        <img src='/Root/global/images/blank.gif'  style="width: 100%; height: 100%; background:url(<%=string.Format("/binaryhandler.ashx?nodeid={0}&propertyname=Binary&width={1}&height={2}", content.Id, viewX, viewY) %>) no-repeat 50% 50%" title='<%=content["DisplayName"] %>' alt="<%=content["DisplayName"] %>" />
                    </a>
   
                <%-- 
                    Thumbnails view
                    ----------------
                    With this few lines of code you can show the images as thumbnails within a bounding box which size has set in the Gallery node itself.
                    If you want to see the difference between the two layout, replace the "Background image view" with the followings:
            
                    <a href="<%=string.Format("/binaryhandler.ashx?nodeid={0}&propertyname=Binary&width={1}&height={2}", content.Id, viewX, viewY) %>" rel="prettyPhoto[gallery1]" title="<%=content["Description"]%>">
                        <img src='<%=string.Format("/binaryhandler.ashx?nodeid={0}&propertyname=Binary&width={1}&height={2}", content.Id, thumbX, thumbY) %>' title='<%=content["DisplayName"] %>' alt="<%=content["DisplayName"] %>" />
                    </a>
                --%>          
                </div>
            </div>
        <% } %>
       
    </div>

</div>

<%-- PAGER --%>
<% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl || this.Model.VisibleFieldNames.Length > 0) { %>
<div class="sn-list-navigation ui-widget-content ui-corner-all ui-helper-clearfix">
    
    <% if (this.Model.VisibleFieldNames.Length > 0) { %>
    <select class="sn-sorter sn-ctrl-select ui-widget-content ui-corner-all" onchange="if (this.value!='') window.location.href=this.value;">
        <option value="">Select ordering...</option>   
        <%foreach (var field in this.Model.VisibleFieldNames) { %>
            <option value="<%=this.Model.SortActions.First(sa => sa.SortColumn == field && sa.SortDescending == false).Url %>"><%=field %> ascending</option>
            <option value="<%=this.Model.SortActions.First(sa => sa.SortColumn == field && sa.SortDescending == true).Url %>"><%=field %> descending</option>
        <% } %>
    </select>
    <%} %>
    
    <% if (((ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl) { %>
    <div class="sn-pager">
        <%foreach (var pageAction in this.Model.Pager.PagerActions) {
        
            if (pageAction.CurrentlyActive) {  %>
                <span class="sn-pager-item sn-pager-active"><%=pageAction.PageNumber%></span>
            <%} else { %>
                <a class="sn-pager-item" href="<%=pageAction.Url %>"><%=pageAction.PageNumber%></a>
            <%} %>
        
        <% } %>
    </div>
    <% } %>
</div>
<% } %>
