<%@ Control Language="C#"%>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ApplicationModel" %>
<%@ Import Namespace="System.Linq" %>

<div class="sn-portalremotecontrol">
        <a runat="server" id="PRCIcon" title="Open Portal Remote Control" class="sn-prc-dock" href="javascript:;">Open PRC</a>
        <a runat="server" id="prctoolbarmenu" visible="false" class="sn-actionlinkbutton icon sn-prc-toolbar sn-prc-toolbar-open" href="javascript:;" title="Open Portal Remote Control">Page Actions</a>

	    <snpe:SNUpdatePanel ID="_updatePageDetails" runat="server" >
	        <ContentTemplate>
	        </ContentTemplate>
		</snpe:SNUpdatePanel>
		
        <div class="sn-prc" id="PortalRemoteControl" title="Portal Remote Control">
		    <div class="sn-prc-body sn-admin-content">
	            
	            <sn:ContextInfo runat="server" ID="ContextInfoPage" Selector="CurrentPage" />
			    <sn:ContextInfo runat="server" ID="ContextInfoContent" Selector="CurrentContent" />
			    <sn:ContextInfo runat="server" ID="ContextInfoAppOrContent" Selector="CurrentApplicationContext" ReplaceNullWithContext="true" />
			    <sn:ContextInfo runat="server" ID="ContextInfoAppOnly" Selector="CurrentApplicationContext" />
			    <sn:ContextInfo runat="server" ID="ContextInfoUrlContent" Selector="CurrentUrlContent" />
			            
		        <asp:Panel id="panelPrcTop" runat="server">
			        <dl class="sn-prc-properties">
			            <dt>Name</dt><dd><asp:Label ID="ContentNameLabel" runat="server" /></dd>
			            <dt>Type</dt><dd><asp:Label ID="ContentTypeLabel" runat="server" /></dd><!--<asp:Image runat="server" ID="ContentTypeImage" />)-->
			            
			            <dt><%=GetGlobalResourceObject("PortalRemoteControl", "VersionLabel")%></dt>
                        <dd><asp:Label ID="VersionLabel" runat="server" ></asp:Label></dd>
			            <dt>Status</dt>
			            <dd>
			                <asp:Label ID="CheckedOutByLabel" runat="server" /><br />
			                <asp:HyperLink runat="server" ID="SendMessageLink" Visible="false" CssClass="sn-prc-sendmsg" ToolTip="Send message">Send message</asp:HyperLink><asp:HyperLink runat="server" ID="CheckedOutLink" Target="_blank" Visible="false" /> 
                            
			            </dd>
			            <dt><%=GetGlobalResourceObject("PortalRemoteControl", "LastModifiedLabel")%></dt>
			            <dd><asp:Label ID="LastModifiedLabel" runat="server" /><br /><asp:HyperLink runat="server" ID="LastModifiedLink" Target="_blank" /></dd>
			            
			            <dt><%=GetGlobalResourceObject("PortalRemoteControl", "PageTemplateLabel")%></dt>
			            <dd><asp:Label ID="PageTemplateLabel" runat="server" /></dd>

			            <dt><%=GetGlobalResourceObject("PortalRemoteControl", "SkinLabel")%></dt>
			            <dd><asp:Label ID="SkinLabel" runat="server" /></dd>
			        </dl>
			    </asp:Panel>
			       
	            <snpe:SNUpdatePanel ID="UpdatePortlet" runat="server" >
                    <Triggers>
                        <asp:PostBackTrigger ControlID="EditModeButton" />
                        <asp:PostBackTrigger ControlID="BrowseModeButton" />
                    </Triggers>
			        <ContentTemplate>
                        <div id="sn-prc-states">
                            <% var urlNodePath = HttpContext.Current.Request.Params[PortalContext.ContextNodeParamName];
                               if (!string.IsNullOrEmpty(urlNodePath))
                               {
                                   var backUrl = PortalContext.Current.BackUrl;
                                   if (!string.IsNullOrEmpty(backUrl))
                                   { %>
                                    <a href='<%= backUrl %>' title="Back to content" class="sn-prc-button sn-prc-tocontent">Application mode</a>
                                <% }
                                   else
                                   { %>
                                <sn:ActionLinkButton ID="BrowseOriginalContent" runat="server" ActionName="Browse" ContextInfoID="ContextInfoUrlContent" IncludeBackUrl="false" CssClass="sn-prc-button sn-prc-tocontent" ToolTip="Back to content" IconVisible="false">Application mode</sn:ActionLinkButton>
                                <% } %>
                            <% } %>
                            <sn:ActionLinkButton ID="BrowseApp" runat="server" ActionName="Browse" ContextInfoID="ContextInfoPage" ParameterString="context={CurrentContextPath}" IncludeBackUrl="true" CssClass="sn-prc-button sn-prc-toapplication" ToolTip="<%$ Resources: PortalRemoteControl, BrowseApp %>" IconVisible="false">Content mode</sn:ActionLinkButton>
                            <asp:LinkButton Visible="false" ID="EditModeButton" CommandName="entereditmode" runat="server" CssClass="sn-prc-button sn-prc-editmode" ToolTip="Switch to Edit mode"><%=GetGlobalResourceObject("PortalRemoteControl", "EditMode")%></asp:LinkButton>
		                    <asp:LinkButton Visible="false" ID="BrowseModeButton" CommandName="enterbrowsemode" runat="server" CssClass="sn-prc-button sn-prc-browsemode" ToolTip="Switch to Preview mode"><%=GetGlobalResourceObject("PortalRemoteControl", "PreviewMode")%></asp:LinkButton>                            
    		                <sn:ActionLinkButton ID="ActionListLink" runat="server" ActionName="ActionList" ContextInfoID="ContextInfoContent" CssClass="sn-prc-button sn-prc-actionlist"  IconVisible="false" Text="All actions" ToolTip="List available actions" />
		                </div>

				        <div id="sn-prc-actions" class="ui-helper-clearfix">
				        <% if (PortalContext.Current.ActionName != null && PortalContext.Current.ActionName.ToLower() == "explore") { %>
                            <sn:ActionLinkButton ActionName="Explore" ID="ExploreRootLink" runat="server" NodePath="/Root" CssClass="sn-prc-button sn-prc-root" IconSize="32" IconName="sn-prc-root"><%=GetGlobalResourceObject("PortalRemoteControl", "RootConsole")%></sn:ActionLinkButton>
				        <% } else {%>
				            <a id="BrowseRoot" href='/Explore.html#/Root' class="sn-prc-button sn-prc-root"><sn:SNIcon ID="SNIcon1" runat="server" Size="32" Icon="sn-prc-root" /><%=GetGlobalResourceObject("PortalRemoteControl", "RootConsole")%></a>
				            <a id="ExploreAdvancedLink" href='/Explore.html#<%= PortalContext.Current.ContextNodePath%>' class="sn-prc-button sn-prc-explore"><sn:SNIcon ID="SNIcon31" Size="32" runat="server" Icon="sn-prc-explore" />Explore</a>
			            <% }%>   
  					       <sn:ActionLinkButton ID="BrowseLink" runat="server" ActionName="Browse" ContextInfoID="ContextInfoContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-browse" Text="Browse" />
					    
                            <sn:ActionLinkButton ActionName="Versions" ID="Versions" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-versions" ><%=GetGlobalResourceObject("PortalRemoteControl", "Versions")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="Edit" ID="EditPage" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32"  IconName="sn-prc-edit-properties"><%=GetGlobalResourceObject("PortalRemoteControl", "Edit")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="SetPermissions" ID="SetPermissions" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-set-permissions"  ><%=GetGlobalResourceObject("PortalRemoteControl", "SetPermissions")%></sn:ActionLinkButton>

                            <sn:ActionLinkButton ActionName="Add" ID="AddLinkButton" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-add-new" ><%=GetGlobalResourceObject("PortalRemoteControl", "CreateNewPageTitle")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="Rename" ID="Rename" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-rename" ><%=GetGlobalResourceObject("PortalRemoteControl", "Rename")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="CopyTo" ID="CopyTo" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-copy" ><%=GetGlobalResourceObject("PortalRemoteControl", "CopyTo")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="MoveTo" ID="MoveTo" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-move" ><%=GetGlobalResourceObject("PortalRemoteControl", "MoveTo")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="Delete" ID="DeletePage" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-delete"><%=GetGlobalResourceObject("PortalRemoteControl", "DeletePageTitle")%></sn:ActionLinkButton>

                            <sn:ActionLinkButton ActionName="CheckOut" ID="CheckoutButton" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-checkout"><%=GetGlobalResourceObject("PortalRemoteControl", "CheckOut")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="CheckIn" ID="CheckinButton" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-checkin"><%=GetGlobalResourceObject("PortalRemoteControl", "CheckIn")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="Publish" ID="PublishButton" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-publish"><%=GetGlobalResourceObject("PortalRemoteControl", "Publish")%></sn:ActionLinkButton>
				            <sn:ActionLinkButton ActionName="Approve" ID="Approve" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-approve"><%=GetGlobalResourceObject("PortalRemoteControl", "Approving")%></sn:ActionLinkButton>
                            <sn:ActionLinkButton ActionName="UndoCheckOut" ID="UndoCheckoutButton" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-undo"><%=GetGlobalResourceObject("PortalRemoteControl", "UndoCheckOut")%></sn:ActionLinkButton>
				            <sn:ActionLinkButton ActionName="ForceUndoCheckOut" ID="ForceUndoCheckOut" runat="server" ContextInfoID="ContextInfoAppOrContent" CssClass="sn-prc-button" IconSize="32" IconName="sn-prc-forceundo"><%=GetGlobalResourceObject("PortalRemoteControl", "ForceUndoCheckOut")%></sn:ActionLinkButton>
			                
			                <asp:Panel id="panelPrcPortletButtons" runat="server">
                                <asp:LinkButton Visible="false" ID="ModifyPortletsButton" CommandName="modifyportlets" runat="server" CssClass="sn-prc-button"><sn:SNIcon ID="SNIcon2" runat="server" Icon="prc-editportlets" /><%=GetGlobalResourceObject("PortalRemoteControl", "ModifyPortlets")%></asp:LinkButton>
			                </asp:Panel>

					    </div>
					    
					    <div id="sn-prc-statusbar" class="ui-corner-all"><span></span><strong id="sn-prc-statusbar-text"></strong></div>
                        <%
                            var context = PortalContext.Current.GetApplicationContext() ?? PortalContext.Current.ContextNode;
                            if (context != null && ActionFramework.GetActions(SenseNet.ContentRepository.Content.Create(context), "Prc", null).Count() != 0)
                        { %>
                            <h3><%=GetGlobalResourceObject("PortalRemoteControl", "CustomActionsTitle")%>:</h3>
                        <% } %>
	                    
	                    <sn:ActionList runat="server" ID="ActionListScenario" Scenario="Prc" ContextInfoID="ContextInfoAppOrContent" WrapperCssClass="sn-prc-customactions" />		        


			        </ContentTemplate>
			    </snpe:SNUpdatePanel>

                <span style="display:none;">
		            <asp:LinkButton ID="AddPortletButton" runat="server" CommandName="addportlet" CssClass="sn-prc-button sn-prc-hiddenaddportlet"><%=HttpContext.GetGlobalResourceObject("PortalRemoteControl", "AddPortlet")%></asp:LinkButton>
		            <asp:TextBox ID="AddPortletButtonTextBox" runat="server" CssClass="sn-prc-button sn-prc-hiddenaddportlettb" />
		        </span>

            </div>
        </div>
</div>
<div id="Message" runat="server" visible="false">PRC Error!</div>




