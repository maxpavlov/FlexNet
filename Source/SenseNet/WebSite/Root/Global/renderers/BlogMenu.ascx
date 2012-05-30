<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<sn:ContextInfo ID="ContextInfoWs" runat="server" Selector="CurrentContext" UsePortletContext="true" />
<sn:ContextInfo ID="ContextInfoParentWs" runat="server" Selector="ParentWorkspace" />

<ul class="sn-menu">
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton0" runat="server" IconVisible="true" ActionName="Browse" UseContentIcon="true" Text="<%$ Resources: Portal, SnBlog_Menu_WorkspaceHome %>" ContextInfoID="ContextInfoParentWs" /></li>
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton1" runat="server" IconVisible="true" ActionName="Browse" UseContentIcon="true" Text="<%$ Resources: Portal, SnBlog_Menu_BlogMainPage %>" ContextInfoID="ContextInfoWs" /></li>    
    <% if(SenseNet.Portal.Helpers.Security.IsInRole("Editors")){ %>
        <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton2" runat="server" IconVisible="true" ActionName="Add" Text="<%$ Resources: Portal, SnBlog_Menu_NewPost %>" ContextInfoID="ContextInfoWs" NodePath="Posts" IncludeBackUrl="true" ParameterString="backtarget=newcontent" /></li>
        <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton3" runat="server" IconVisible="true" ActionName="UnpublishedPosts" Text="<%$ Resources: Portal, SnBlog_Menu_UnpublishedPosts %>" ContextInfoID="ContextInfoWs" /></li>
    <% } %>
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton4" runat="server" IconVisible="true" ActionName="Search" Text="<%$ Resources: Portal, SnBlog_Menu_Search %>" ContextInfoID="ContextInfoWs" IncludeBackUrl="false" /></li>
</ul>