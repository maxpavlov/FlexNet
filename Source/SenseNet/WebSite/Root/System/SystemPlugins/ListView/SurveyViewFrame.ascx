<%@ Import Namespace="SenseNet.ContentRepository"%>
<%@ Import Namespace="SenseNet.ContentRepository.Storage"%>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ViewFrame" %>
<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />
<sn:ContextInfo runat="server" Selector="CurrentList" UsePortletContext="true" ID="myList" />

<div class="sn-listview">
    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="Left" runat="server">
            <sn:ActionMenu runat="server" Scenario="New" ContextInfoID="myContext" RequiredPermissions="AddNew">
                <sn:ActionLinkButton runat="server" ActionName="Add" IconUrl="/Root/Global/images/icons/16/newfile.png" ContextInfoID="myContext" Text="New" />
                <%-- a href="<% =ResolveAction(MostRelevantContext.Path, "Add") %>">New</a --%>
            </sn:ActionMenu>
            <sn:ActionLinkButton runat="server" ActionName="Upload" IconUrl="/Root/Global/images/icons/16/upload.png" ContextInfoID="myContext" Text="Upload" />
            <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/wizard.png" Scenario="ListActions" ContextInfoID="myContext" CheckActionCount="True">Actions</sn:ActionMenu>
            <sn:ActionLinkButton CssClass="sn-batchaction" runat="server" ActionName="CopyBatch" IconUrl="/Root/Global/images/icons/16/copy.png" ContextInfoID="myContext" Text="Copy selected..." ParameterString="{PortletClientID}" />
            <sn:ActionLinkButton CssClass="sn-batchaction" runat="server" ActionName="MoveBatch" IconUrl="/Root/Global/images/icons/16/move.png" ContextInfoID="myContext" Text="Move selected..." ParameterString="{PortletClientID}" />
            <sn:ActionLinkButton CssClass="sn-batchaction" runat="server" ActionName="DeleteBatch" ContextInfoID="myContext" Text="Delete selected..." ParameterString="{PortletClientID}" />
            <sn:ActionLinkButton CssClass="sn-batchaction" runat="server" ActionName="ContentLinkBatch" ContextInfoID="myContext" Text="Create content links" ParameterString="{PortletClientID}" />
        </sn:ToolbarItemGroup>
        <sn:ToolbarItemGroup runat="server" Align="Right">
            <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/settings.png" Scenario="SurveySettings" ContextInfoID="myList" CheckActionCount="True">Settings</sn:ActionMenu>
            <span class="sn-actionlabel">View:</span>
            <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/views.png" Scenario="Views" ContextInfoID="myList" CheckActionCount="True" ScenarioParameters="{PortletID}" >
              <% =SenseNet.Portal.UI.ContentListViews.ViewManager.LoadViewInContext(ContextNode, LoadedViewName).DisplayName%>
            </sn:ActionMenu>
         </sn:ToolbarItemGroup>   
    </sn:Toolbar>
    <asp:Panel CssClass="sn-listview-checkbox" ID="ListViewPanel" runat="server"></asp:Panel>
</div>
