<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Schema" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<div class="sn-contentlist sn-bloglist">
    <%
        if (this.Model.Items.Count() == 0)
        {%>
        <div class="sn-blogpost">
            <p class="sn-blogpost-missing"><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_NoPosts")%></p>
        </div>
        <%}
        else
        {
            var wsContent = SenseNet.ContentRepository.Content.Load(this.Model.Content.WorkspacePath);
            bool showAvatar = wsContent["ShowAvatar"].Equals(true);
            foreach (var content in this.Model.Items)
            {
    %>
    <div class="sn-blogpost">
        <div>
            <% if (showAvatar)
               { %>
                <div style="float: left; margin-right: 10px;">
                    <img class="sn-blogpost-avatar" style="margin-top: 5px" src='<%= UITools.GetAvatarUrl(content["CreatedBy"] as SenseNet.ContentRepository.User) %>?dynamicThumbnail=1&width=48&height=48' alt='<%# Eval("CreatedBy") %>' />
                </div>
            <%} %>            
            <div>
                <div class="sn-blogpost-admin">
                    <% var editUrl = Actions.ActionUrl(content, "Edit", true);
                       var deleteUrl = Actions.ActionUrl(content, "Delete", true); 
                       if(!String.IsNullOrEmpty(editUrl))
                       {%>
                          <div class="sn-blogpost-admin-item"><img src="/Root/Global/images/icons/16/edit.png" alt="Edit" /><a href="<%= editUrl %>"><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_EditPost")%></a></div>
                       <%}
                       if (!String.IsNullOrEmpty(deleteUrl))
                       {%>
                           <div class="sn-blogpost-admin-item"><img src="/Root/Global/images/icons/16/delete.png" alt="Delete" /><a href="<%= deleteUrl %>"><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_DeletePost")%></a></div>
                       <%}
                    %>                    
                </div>
                <div class="sn-blogpost-info">
                    <h1 class="sn-blogpost-title">
                        <%=Actions.BrowseAction(content, true)%>
                    </h1>
                    <span class="sn-blogpost-createdby"><%=(content["CreatedBy"] as SenseNet.ContentRepository.User).FullName%></span> 
                    <span class="sn-blogpost-publishedon"><%=content["PublishedOn"]%></span>
                    <%  var i = 0;
                        foreach (string tag in content["Tags"].ToString().Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries)) {
                        i++;
                    %>
                        <%= i > 1 ? ", " : " - " %>
                        <a class="sn-blogpost-tag" href="<%= Actions.ActionUrl(wsContent, "Search", false) + "&text=" + HttpUtility.UrlEncode(tag) %>"><%= tag %></a><% } %>
                 </div>
            </div>
            
        </div>
        <div class="sn-blogpost-lead">
            <%=content["LeadingText"]%>
            <% if (!String.IsNullOrEmpty(content["BodyText"].ToString()))
               {%>
                <a class="sn-blogpost-readmore" href="<%=Actions.BrowseUrl(content, true) %>"><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_ReadMore")%></a>
            <%} %>
        </div>       
        
        <div class="sn-blogpost-footer">
            <div class="sn-blogpost-comments">
            <% 
                var commentCount = SenseNet.Portal.Wall.CommentInfo.GetCommentCount(content.Id);
                if (commentCount > 0) { 
                    %><strong><a href="<%= Actions.ActionUrl(content, "Browse", true) %>"><%= String.Format(SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_Comments"), commentCount) %></a></strong><%
                }
                else {                     
                    %><i><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_NoComments")%></i><%
                }
            %>                
            </div>
        </div>
        <div class="sn-clearfix"></div>
    </div>
    <%} %>

    <% if (((SenseNet.Portal.Portlets.ContentCollectionPortlet)this.Model.State.Portlet).ShowPagerControl && this.Model.Pager.Pagecount > 1)
       { %>
    <div class="sn-pager sn-blog-pager">
        <%foreach (var pageAction in this.Model.Pager.PagerActions)
          {

              if (pageAction.CurrentlyActive)
              {  %>
                <span class="sn-pager-item sn-pager-active"><%=pageAction.PageNumber%></span>
            <%}
              else
              { %>
                <a class="sn-pager-item" href="<%=pageAction.Url %>"><%=pageAction.PageNumber%></a>
            <%} %>
        
        <% } %>
    </div>
    <% }
        } %>

</div>