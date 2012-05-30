<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>

<sn:ContextInfo ID="ContextInfoWs" runat="server" Selector="CurrentContext" UsePortletContext="true" />
<sn:ContextInfo ID="ContextInfoParentWs" runat="server" Selector="ParentWorkspace" />

<ul class="sn-menu">
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton0" runat="server" IconVisible="true" ActionName="Browse" UseContentIcon="true" Text="Workspace home" ContextInfoID="ContextInfoParentWs" /></li>
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton1" runat="server" IconVisible="true" ActionName="Browse" UseContentIcon="true" Text="Wiki main page" ContextInfoID="ContextInfoWs" /></li>
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton2" runat="server" IconVisible="true" ActionName="Browse" IconName="Versions" Text="Recent changes" ContextInfoID="ContextInfoWs" NodePath="RecentChanges" /></li>
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton3" runat="server" IconVisible="true" ActionName="Browse" IconName="Image" Text="Images" ContextInfoID="ContextInfoWs" NodePath="Images" /></li>
    <% var aaa = PortalContext.Current.ActionName == null ? "" : PortalContext.Current.ActionName.ToLower();
       if (aaa != "add" && aaa != "upload")
       { %>
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton4" runat="server" IconVisible="true" ActionName="Add" Text="New article" ContextInfoID="ContextInfoWs" NodePath="Articles" IncludeBackUrl="true" ParameterString="backtarget=newcontent" /></li>
    <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton5" runat="server" IconVisible="true" ActionName="Upload" Text="Upload image" ContextInfoID="ContextInfoWs" NodePath="Images" /></li>    
    <% } %>
</ul>
