<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>

<sn:ContextInfo runat="server" Selector="CurrentUser" UsePortletContext="true" ID="myContext" />

<sn:ActionLinkButton ContextInfoID='myContext' IconPath="/Root/Global/images/icons/16/user.png" ActionName="Profile" Text="Visit your profile" runat="server" />&nbsp;
<a class="sn-actionlinkbutton" href="/"><img src="/Root/Global/images/icons/16/Site.png" class="sn-icon sn-icon16" alt="Home" />Site home</a>