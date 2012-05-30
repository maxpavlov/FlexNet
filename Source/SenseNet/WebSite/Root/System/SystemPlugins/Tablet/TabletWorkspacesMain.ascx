<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %> 
<%@ Import Namespace="SenseNet.ContentRepository" %> 
<%@ Import Namespace="SenseNet.ContentRepository.Workspaces" %> 
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Search" %> 
<% 
var cc = PortalContext.Current.ContextNode;
var wsListCritical = ContentQuery.Query("+InTree:\"@@CurrentContent.Path@@\" -Id:@@CurrentContent@@ +TypeIs:Workspace -TypeIs:Wiki +IsCritical:true").Nodes.Cast<Workspace>();
var wsListNonCritical = ContentQuery.Query("+InTree:\"@@CurrentContent.Path@@\" -Id:@@CurrentContent@@ +TypeIs:Workspace -TypeIs:Wiki -Critical:true").Nodes.Cast<Workspace>();
var taskList = ContentQuery.Query("+InTree:\"@@CurrentContent.Path@@\" +TypeIs:Task +AssignedTo:@@CurrentUser@@ .SORT:DueDate .TOP:10").Nodes.Cast<Task>();
var docList = ContentQuery.Query("+InTree:\"@@CurrentContent.Path@@\" +TypeIs:File +CreatedBy:@@CurrentUser@@ .REVERSESORT:ModificationDate .TOP:10").Nodes.Cast<File>();
%>

<article class="snm-tile bg-zero" id="reload">
    <a href="javascript:location.reload(true)" class="snm-link-tile bg-zero clr-text">
        <span class="snm-lowertext snm-fontsize3">Refresh</span>
    </a>
</article>
<article class="snm-tile" id="backtile">
    <a href="javascript:window.history.back()" class="snm-link-tile bg-semitransparent clr-text">
        <span class="snm-lowertext snm-fontsize3">Back</span>
    </a>
</article>
<div id="snm-container">
    <div id="page1" class="snm-page">
        <div class="snm-pagecontent">
       	    <div class="snm-col">
    		    <h1 class="anim-slidein">Welcome <%= User.Current.FullName %>!</h1>

                <h2 class="anim-slidein">Critical Workspaces (<%= wsListCritical.ToArray().Length%>)</h2>
                <% foreach (var ws in wsListCritical)
                    { %>
                    <article class="snm-tile snm-tile-wide3 bg-primary clr-text">
                        <a href="<%= Actions.BrowseUrl(SenseNet.ContentRepository.Content.Create(ws)) %>" class="snm-link-tile"><span class="snm-lowertext snm-fontsize3"><%= ws.DisplayName %></span></a>
                    </article>
                <% } %>

                <h2 class="anim-slidein">Other Workspaces (<%= wsListNonCritical.ToArray().Length%>)</h2>
                <% foreach (var ws in wsListNonCritical)
                   { %>
                    <article class="snm-tile snm-tile-wide3 bg-primary clr-text anim-zoomin">
                        <a href="<%= Actions.BrowseUrl(SenseNet.ContentRepository.Content.Create(ws)) %>" class="snm-link-tile"><span class="snm-lowertext snm-fontsize3"><%= ws.DisplayName %></span></a>
                    </article>
                <% } %>

                <h2 class="anim-slidein">My tasks (<%= taskList.ToArray().Length%>)</h2>
                <% foreach (var task in taskList)
                    { %>
                    <article class="snm-tile snm-tile-wide3 bg-primary clr-text anim-zoomin">
                        <a href="<%= Actions.BrowseUrl(SenseNet.ContentRepository.Content.Create(task)) %>" class="snm-link-tile"><span class="snm-lowertext snm-fontsize3"><%= task.DisplayName %></span></a>
                    </article>
                <% } %>

                <h2 class="anim-slidein">My documents (<%= docList.ToArray().Length%>)</h2>
                <% foreach (var doc in docList)
                    { %>
                    <article class="snm-tile snm-tile-wide3 bg-primary clr-text anim-zoomin">
                        <a href="<%= Actions.BrowseUrl(SenseNet.ContentRepository.Content.Create(doc)) %>" class="snm-link-tile"><span class="snm-lowertext snm-fontsize3"><%= doc.DisplayName %></span></a>
                    </article>
                <% } %>

            </div>
        </div>
    </div>
</div>
