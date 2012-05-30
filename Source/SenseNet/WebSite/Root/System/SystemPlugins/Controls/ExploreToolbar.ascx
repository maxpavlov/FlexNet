<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />

<sn:Toolbar runat="server">
    <sn:ToolbarItemGroup Align="Left" runat="server">
        <sn:ActionList runat="server" Scenario="ExploreToolbar" ContextInfoID="myContext" ControlPath="/Root/System/SystemPlugins/Controls/ActionToolbar.ascx" />
        <sn:ToolbarSeparator runat="server" />
        <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/wizard.png" Scenario="ExploreActions" ContextInfoID="myContext" Text="Actions">Actions</sn:ActionMenu>
    </sn:ToolbarItemGroup>
    <sn:ToolbarItemGroup runat="server" Align="Right">
        <asp:PlaceHolder ID="PRCEcms" runat="server" />            
     </sn:ToolbarItemGroup>   
</sn:Toolbar>