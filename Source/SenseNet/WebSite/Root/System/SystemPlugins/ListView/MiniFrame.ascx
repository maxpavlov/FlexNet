<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ViewFrame" %>

<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />

<div class="sn-listview">
<% string user = (SenseNet.ContentRepository.User.Current).ToString(); %>
<%if (user == "Visitor")
  {%>
   <div class="sn-pt-body-border ui-widget-content ui-corner-all">
	<div class="sn-pt-body ui-corner-all">
		<%=GetGlobalResourceObject("Portal", "WSContentList_Visitor")%>
	</div>
</div>
<% }%>
<%else
  {%>
   <sn:Toolbar ID="Toolbar1" runat="server">
    <sn:ToolbarItemGroup ID="ToolbarItemGroup1" Align="Left" runat="server">
            <sn:ActionMenu ID="ActionMenu1" runat="server" Scenario="New" ContextInfoID="myContext" RequiredPermissions="AddNew" CheckActionCount="True">
                <sn:ActionLinkButton ID="ActionLinkButton1" runat="server" ActionName="Add" IconUrl="/Root/Global/images/icons/16/newfile.png" ContextInfoID="myContext" Text="New" CheckActionCount="True"/>
            </sn:ActionMenu>
    </sn:ToolbarItemGroup>   
  </sn:Toolbar>
  <asp:Panel ID="ListViewPanel" runat="server">
  </asp:Panel>

  <%} %>
</div>


