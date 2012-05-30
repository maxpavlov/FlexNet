<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.PortletFramework" TagPrefix="cbp" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.Portlets" TagPrefix="nep" %>

<% var targetContent = SenseNet.ContentRepository.Content.Load(this.Content["ContentPath"] as string); %>

<div class="sn-content-inlineview-header ui-helper-clearfix">
    <%= targetContent == null ? string.Empty : SenseNet.Portal.UI.IconHelper.RenderIconTag(targetContent.Icon, null, 32)%>
    <div class="sn-content-info sn-notification-content">
        <h2 class="sn-view-title"><%= targetContent == null ? "Unknown content" : targetContent.DisplayName %>
        <% if ( !(bool)(ContextBoundPortlet.GetContainingContextBoundPortlet(this) as NotificationEditorPortlet).IsSubscriptionNew ) {  %>
            <i> (edit existing notification)</i>
        <% } %></h2>
        <strong>Path:</strong> <%= this.Content["ContentPath"] %>
    </div>
          
    <div class="sn-infobox">
        <img class="sn-icon sn-icon_32" src="/Root/Global/images/icons/32/info.png" alt="" />
        You will receive notification emails if this content is changed or deleted. 
        <br />
        In case of a container type (workspace, list or folder) you will receive an 
        email if new content is created or deleted under this content as well.
    </div>
</div>

<% if (!string.IsNullOrEmpty(this.Content["UserEmail"] as string)) {  %>
    <sn:ShortText ID="UserEmail" runat="server" FieldName="UserEmail" ControlMode="Browse" />
<% } %>

<sn:RadioButtonGroup ID="DrpDwnFrequency" runat="server" FieldName="Frequency" />

<% if ( !(bool)(ContextBoundPortlet.GetContainingContextBoundPortlet(this) as NotificationEditorPortlet).IsSubscriptionNew ) {  %>
    <sn:Boolean ID="BoolIsActive" runat="server" FieldName="IsActive" />
<% } %>

<sn:DropDown ID="DrpDwnLang" runat="server" FieldName="Language" />

<div class="sn-panel sn-buttons">
    <% if (!string.IsNullOrEmpty(this.Content["UserEmail"] as string)) {  %>
        <asp:Button ID="BtnSave" runat="server" CssClass="sn-submit" Text="Save" />
    <% } else { %>    
        <span class="sn-error"><%= GetGlobalResourceObject("Notification", "MissingEmailAddressError")%></span>
    <% } %>

    <sn:BackButton ID="BackButton" runat="server" class="sn-submit" Text="Cancel" />  
</div>​