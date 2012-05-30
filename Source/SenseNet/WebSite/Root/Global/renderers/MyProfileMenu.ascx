<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>

<% var profile = PortalContext.Current.ContextWorkspace as UserProfile;
   var profileUser = profile.User as User;

   if (User.Current.Id == profileUser.Id)
   { %>
<sn:ContextInfo ID="ContextInfoProfile" runat="server" Selector="CurrentContext" UsePortletContext="true" />
<sn:ContextInfo ID="ContextInfoCurrent" runat="server" Selector="CurrentContext" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfoUser" runat="server" Selector="CurrentContext" UsePortletContext="true" ReferenceFieldName="User" />


<sn:ContextInfo ID="ContextInfo1" runat="server" Selector="currentuser" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfo2" runat="server" Selector="currentpage" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfo3" runat="server" Selector="currentsite" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfo4" runat="server" Selector="currentlist" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfo5" runat="server" Selector="currentworkspace" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfo6" runat="server" Selector="parentworkspace" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfo7" runat="server" Selector="currentapplicationcontext" UsePortletContext="false" />
<sn:ContextInfo ID="ContextInfo8" runat="server" Selector="currenturlcontent" UsePortletContext="false" />


<div class="sn-pt-border ui-widget-content ui-corner-all">
    <div class="sn-pt-body-border ui-widget-content ui-corner-all">
        <div class="sn-pt-body ui-corner-all">
            <ul class="sn-menu">
                <li class="sn-menu-0 sn-index-0">
                    <a class="sn-actionlink" href="/" title="[home]">
                        <img class="sn-icon sn-icon16" title="" alt=[home] src="/Root/Global/images/icons/16/site.png" />
                        Site home</a></li>
                <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton0" runat="server" IconVisible="true" ActionName="Edit" UseContentIcon="true" Text="Edit profile" ContextInfoID="ContextInfoUser" /></li>
                <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton1" runat="server" IconVisible="true" ActionName="Notifications" Text="Notifications" ContextInfoID="ContextInfoUser" /></li>
                <li class="sn-menu-0 sn-index-0"><sn:ActionLinkButton ID="alButton2" runat="server" IconVisible="true" ActionName="SetPermissions" Text="Permissions" ContextInfoID="ContextInfoCurrent" /></li>
            </ul>
        </div>
    </div>
</div>

<% } %>