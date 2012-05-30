<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>

<sn:ContextInfo runat="server" ID="ContextInfoContent" Selector="CurrentContent" />

<div class="sn-content sn-content-inlineview">
        <div class="sn-inputunit ui-helper-clearfix">
            <div class="sn-iu-label">
                <div class="sn-iu-title">Document Library name</div>                    
            </div>
            <div class="sn-iu-control">
                <%= PortalContext.Current.ContextNode.Name %>
            </div>
            <div class="sn-iu-label">
                <div class="sn-iu-title">Document Library title</div>
            </div>
            <div class="sn-iu-control">
                <%= ((SenseNet.ContentRepository.GenericContent)PortalContext.Current.ContextNode).DisplayName %>
            </div>
            <div class="sn-iu-label">
                <div class="sn-iu-title">Path</div>
            </div>
            <div class="sn-iu-control">
                <%= PortalContext.Current.ContextNodePath %>
            </div>
            <div class="sn-iu-label">
                <div class="sn-iu-title">Description</div>
            </div>
            <div class="sn-iu-control">
                <%= PortalContext.Current.ContextNode["Description"] %>
            </div>
        </div>
    
        <h2 class="sn-content-title">Choose from the following configuration pages</h2>
    
        <div class="sn-inputunit ui-helper-clearfix">
            <div class="sn-iu-label">
                <span class="sn-iu-title">Settings</span><br />
                <span class="sn-iu-desc">Use these dialogs to edit the advanced settings of the Document library</span>
            </div>
            <div class="sn-iu-control">
                <ul class="sn-list">
                    <li><sn:ActionLinkButton CssClass="sn-link" ActionName="Edit" ContextInfoID="ContextInfoContent" ID="EditLink" runat="server" IconVisible="False" >General settings (Name, Versioning, etc.)</sn:ActionLinkButton></li>
                    <li><sn:ActionLinkButton CssClass="sn-link" ActionName="ManageFields" ContextInfoID="ContextInfoContent" ID="ManageFieldsLink" runat="server" IconVisible="False" >Manage Fields (Add, edit, delete)</sn:ActionLinkButton></li>
                    <li><sn:ActionLinkButton CssClass="sn-link" ActionName="ManageViews" ContextInfoID="ContextInfoContent" ID="ManageViewsLink" runat="server" IconVisible="False" >Manage Views</sn:ActionLinkButton></li>
                    <li><sn:ActionLinkButton CssClass="sn-link" ActionName="ManageWorkflows" ContextInfoID="ContextInfoContent" ID="ManageWorkflowsLink" runat="server" IconVisible="False" >Manage Workflows</sn:ActionLinkButton></li>
                    <li><sn:ActionLinkButton CssClass="sn-link" ActionName="IncomingEmailSettings" ContextInfoID="ContextInfoContent" ID="IncomingEmailSettingsLink" runat="server" IconVisible="False" >Incoming email settings</sn:ActionLinkButton></li>
                    <!-- <li><a class="sn-actionlinkbutton">Manage Templates</a></li> -->
                </ul>
            </div>
        </div>
    
    <div class="sn-panel sn-buttons">
      <sn:BackButton Text="Done" runat="server" ID="BackButton" CssClass="sn-submit" />
    </div>
    
</div>
