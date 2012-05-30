<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %> 
<%@ Import Namespace="SenseNet.ContentRepository" %> 
<%@ Import Namespace="SenseNet.ContentRepository.Workspaces" %> 
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Search" %> 
<% 
    var cc = PortalContext.Current.ContextNode;
    var manager = cc.GetReference<User>("Manager");
    
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
    		    <h1><%= cc.DisplayName %></h1>

                <article class="snm-tile snm-clip bg-primary clr-text">
                    <span class="snm-progress"><span class="snm-progress-bar" style="width:<%= cc["Completion"] %>%"></span></span>
                    <span class="snm-bigtext"><%= cc["Completion"] %>%</span>
                </article>

                <% if (cc["Deadline"] != null) { %>
                <article class="snm-tile snm-calendar bg-primary clr-text">
                    <span class="snm-month"><%= ((DateTime)cc["Deadline"]).ToString("MMM")%></span> <span class="snm-day"><%= ((DateTime)cc["Deadline"]).ToString("%d") %></span>
                </article>
                <% } %>

                <% if (manager != null) { %>
                <article class="snm-tile snm-flip">
                    <section class="snm-front">
                        <span class="snm-background"><img src="<%= SenseNet.Portal.UI.UITools.GetAvatarUrl(manager) %>?dynamicThumbnail=1&width=80&height=80" alt="" title="<%= manager["FullName"] %>" /></span>
                    </section>
                    <section class="snm-back bg-primary">
                        <span class="snm-lowertext snm-fontsize3"><a href="mailto:<%= manager["Email"] %>"><%= manager["FullName"] %></a></span>
                    </section>
                </article>
                <% } %>

                <article class="snm-tile snm-tile-wide2 snm-flip snm-clock">
                    <section class="snm-front snm-state-highlight">
                        <span class="snm-middletext snm-fontsize2">FLIP - FRONT</span>
                    </section>
                    <section class="snm-back bg-primary">
                        <span class="snm-middletext snm-fontsize2">FLIP - BACK</span>
                    </section>
                </article>


            </div>
        </div>
    </div>
</div>
