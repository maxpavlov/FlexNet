<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.GenericContentView" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Versioning" %>

<div class="sn-content-inlineview-header ui-helper-clearfix">
    <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(Content.Icon, null, 32) %>
	<div class="sn-content-info">
        <h2 class="sn-view-title"><% = DisplayName %> (<%= ContentType.DisplayName %>)</h2>
        <strong>Path:</strong> <%= ContentHandler.Path %>
    <% var gc = ContentHandler as GenericContent;
       if (gc.VersioningMode > VersioningType.None || gc.ApprovingMode == ApprovingType.True || gc.Locked || gc.Version.Major > 1) { %>
       <br /><strong>Version:</strong> <%= ContentHandler.Version.ToDisplayText() %>
    <% } %>
    </div>
</div>
<div id="InlineViewContent" runat="server" class="sn-content sn-content-inlineview">
        [GENERIC CONTENT PLACEHOLDER]
</div>
<asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>

<div class="sn-panel sn-buttons">
  <sn:CommandButtons ID="CommandButtons1" runat="server" layoutControlPath="/Root/System/SystemPlugins/Controls/CommandButtons.ascx" />
</div>