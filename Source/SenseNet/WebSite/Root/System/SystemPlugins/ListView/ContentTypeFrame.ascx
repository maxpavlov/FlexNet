<%@ Import Namespace="SenseNet.ContentRepository"%>
<%@ Import Namespace="SenseNet.ContentRepository.Storage"%>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ViewFrame" %>
<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />
<sn:ContextInfo runat="server" Selector="CurrentList" UsePortletContext="true" ID="myList" />

<div class="sn-listview">
    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="Left" runat="server">
            <sn:ActionLinkButton runat="server" ActionName="Add" ParameterString="ContentTypeName=ContentType" IconUrl="/Root/Global/images/icons/16/newfile.png" ContextInfoID="myContext" Text="New" />
            <sn:ActionLinkButton CssClass="sn-batchaction" runat="server" ActionName="DeleteBatch" ContextInfoID="myContext" Text="Delete selected..." ParameterString="{PortletClientID}" />
        </sn:ToolbarItemGroup>   
        <sn:ToolbarItemGroup runat="server" Align="Right">
            <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/settings.png" Scenario="Settings" ContextInfoID="myList">Settings</sn:ActionMenu>
            <span class="sn-actionlabel">View:</span>
            <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/views.png" Scenario="Views" ContextInfoID="myList"
                ScenarioParameters="{PortletID}" >
              <% =SenseNet.Portal.UI.ContentListViews.ViewManager.LoadViewInContext(ContextNode, LoadedViewName).DisplayName%>
            </sn:ActionMenu>
        </sn:ToolbarItemGroup>   
    </sn:Toolbar>
    <asp:Panel CssClass="sn-listview-checkbox" ID="ListViewPanel" runat="server">
    </asp:Panel>
</div>
