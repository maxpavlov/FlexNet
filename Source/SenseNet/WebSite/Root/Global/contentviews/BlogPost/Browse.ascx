<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Schema" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<%var wsContent = SenseNet.ContentRepository.Content.Load(this.Content.WorkspacePath); %>

<div class="sn-contentlist sn-blogpost-browse">
    <div class="sn-blogpost ui-helper-clearfix">
        <% 
            var unpublished = Content["IsPublished"].Equals(false) || (DateTime)Content["PublishedOn"] >= DateTime.Now;
            if (unpublished && !Security.IsInRole("Editors"))
            {%>
                <div class="sn-blogpost-unpublished">                    
                    <span><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_UnpublishedPost")%></span>
                </div>
            <%}
            else
            {
                if (unpublished){ %>
                    <div class="sn-blogpost-unpublished">                    
                        <span><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_BlogPostsPortlet_UnpublishedPost")%></span>
                    </div>
                <%} 
                bool showAvatar = Node.LoadNode(this.Content.WorkspacePath)["ShowAvatar"].Equals(1);
                if (showAvatar)
                { %>
                <div style="float: left; margin-right: 10px;"><img style="margin-top: 5px" src='<%= UITools.GetAvatarUrl(this.Content["CreatedBy"] as SenseNet.ContentRepository.User) %>?dynamicThumbnail=1&width=48&height=48' class='sn-entry-avatar' alt='<%# Eval("CreatedBy") %>' title='' /></div>
                <%} %>  
                <div>
                    <div class="sn-blogpost-admin">
                        <% var editUrl = Actions.ActionUrl(this.Content, "Edit", true);
                           var deleteUrl = Actions.ActionUrl(this.Content, "Delete", true);
                           if (!String.IsNullOrEmpty(editUrl))
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
                        <h1 class="sn-blogpost-title"><%= GetValue("DisplayName")%></h1>
                        <span class="sn-blogpost-createdby"><%= (this.Content["CreatedBy"] as SenseNet.ContentRepository.User).FullName%></span> - 
                        <span class="sn-blogpost-publishedon"><%= GetValue("PublishedOn")%></span>
                        <% var i = 0;
                           foreach (string tag in this.Content["Tags"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                           { 
                            i++;
                            %><%= i > 1 ? ", " : " - " %><a class="sn-blogpost-tag" href="<%= Actions.ActionUrl(wsContent, "Search", false) + "&text=" + HttpUtility.UrlEncode(tag) %>"><%= tag%></a><%} %>
                     </div>
                </div>
                <div class="sn-blogpost-lead"><%= GetValue("LeadingText")%></div>
                <div class="sn-blogpost-body"><%= GetValue("BodyText")%></div>
        <%}%>
    </div>
    <div class="sn-panel sn-buttons">
        <sn:BackButton ID="BackButton1" runat="server" Text="<%$ Resources: Portal, SnBlog_BlogPostsPortlet_BackToMain %>" CssClass="sn-submit"/>
    </div>
</div>
